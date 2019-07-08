FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
import(path : "ea135cd06b50b2da6bf2aafe", version : "9482ed384753162ac07f0b06");

annotation { "Feature Type Name" : "Clover" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Face", "Filter" : EntityType.FACE, "MaxNumberOfPicks" : 1 }
        definition.parameter is Query;
        
        annotation { "Name" : "Scale" }
        isInteger(definition.scale, POSITIVE_COUNT_BOUNDS);
        
        //Originally intended just for testing since 2D sketch is already defined
        //User currently only has option to extrude on selected face
        //Extrude GUI tool is already sufficient enough.  Clover is recognized by software 
        annotation { "Name" : "3D" }
        definition.myBoolean is boolean;
        
        annotation { "Name" : "Depth" }
        isLength(definition.depth, { (millimeter) : [1, 2, 9000] } as LengthBoundSpec);
        
    }
    {
        //Focusing on the right part of the Clover Symbol, it was split into 9 sections for the program
        //Each variable below represents those parts.  The end points for each are the same for 
        //the beginning of subsequent part.
        
        //Points imported as DXF and added manually
        //https://cad.onshape.com/documents/937545479d56780f57c907dc/w/889edb79cb60ba7ac82416d5/e/ea135cd06b50b2da6bf2aafe
        
        //emblemSections is a variable that keeps track of how many sections the Clover Image is split into
        //Can add more sections, or split them up as desired
        var emblemSections = 9;
        var emblemS1 = [vector(398, 904) * millimeter, vector(484, 785) * millimeter, vector(572, 682) * millimeter];
        var emblemS2 = [vector(572, 682) * millimeter, vector(516, 620) * millimeter, vector(460, 540) * millimeter];
        var emblemS3 = [
            vector(460, 540) * millimeter, vector(495, 567) * millimeter, vector(530, 589) * millimeter,
            vector(565, 607) * millimeter, vector(621, 607) * millimeter, vector(675, 588) * millimeter,
            vector(710, 551) * millimeter, vector(730, 501) * millimeter, vector(733, 457) * millimeter,
            vector(722, 422) * millimeter, vector(706, 389) * millimeter, vector(678, 360) * millimeter,
            vector(635, 342) * millimeter, vector(592, 348) * millimeter, vector(561, 373) * millimeter
        ];
        var emblemS4 = [
            vector(561, 373) * millimeter, vector(575, 390) * millimeter, vector(581, 418) * millimeter,
            vector(569, 444) * millimeter, vector(547, 460) * millimeter, vector(524, 463) * millimeter,
            vector(497, 451) * millimeter, vector(473, 431) * millimeter, vector(462, 415) * millimeter,
            vector(455, 402) * millimeter
        ];
        var emblemS5 = [
            vector(455, 402) * millimeter, vector(456, 402) * millimeter, vector(457, 398) * millimeter,
            vector(460, 392) * millimeter, vector(460, 385) * millimeter, vector(460, 380) * millimeter,
            vector(459, 378) * millimeter, vector(456, 375) * millimeter, vector(459, 367) * millimeter,
            vector(458, 361) * millimeter, vector(455, 354) * millimeter, vector(452, 351) * millimeter,
            vector(444, 347) * millimeter
        ];
        var emblemS6 = [
            vector(444, 347) * millimeter, vector(447, 339) * millimeter, vector(453, 330) * millimeter,
            vector(460, 322) * millimeter, vector(471, 317) * millimeter, vector(484, 317) * millimeter,
            vector(495, 320) * millimeter, vector(500, 327) * millimeter, vector(504, 335) * millimeter,
            vector(504, 342) * millimeter
        ];
        var emblemS7 = [
            vector(504, 342) * millimeter, vector(515, 346) * millimeter, vector(529, 340) * millimeter,
            vector(542, 330) * millimeter, vector(553, 313) * millimeter, vector(554, 298) * millimeter,
            vector(554, 285) * millimeter, vector(550, 269) * millimeter, vector(542, 253) * millimeter,
            vector(525, 239) * millimeter, vector(503, 235) * millimeter, vector(488, 236) * millimeter,
            vector(476, 238) * millimeter, vector(463, 245) * millimeter, vector(452, 254) * millimeter,
            vector(435, 265) * millimeter
        ];
        var emblemS8 = [vector(435, 265) * millimeter, vector(456, 244) * millimeter, vector(479, 226) * millimeter];
        var emblemS9 = [
            vector(479, 226) * millimeter, vector(450, 187) * millimeter, vector(421, 158) * millimeter,
            vector(397, 123) * millimeter
        ];

        //Two arrays to represent the right and left part of the emblem.  Due to issues using the mirror feature on Featurescript
        //to specify the right plane when the user selects on a 3D model's face (for instance), vectors were used instead of specifying a
        //Onshape plane.
        var emblemRight = [emblemS1, emblemS2, emblemS3, emblemS4, emblemS5, emblemS6, emblemS7, emblemS8, emblemS9];
        var emblemLeft = [emblemS1, emblemS2, emblemS3, emblemS4, emblemS5, emblemS6, emblemS7, emblemS8, emblemS9];

        
        emblemRight = shiftFlipScale(context, emblemRight, definition.scale, -398, -504, 0);
        emblemLeft = shiftFlipScale(context, emblemLeft, definition.scale, 398, -504, 1);
        
        //Creates the 2D sketch
        sketchEmblem(context, id, emblemLeft, emblemRight, emblemSections, definition.parameter);
        
        //Boolean check box for built-in 3D options
        if (definition.myBoolean == true) {
            try {
                extrudeEmblem(context, id, "sketch1", definition.parameter, definition.depth);
            }
        }
    });
    
    
    //shiftAndScale :
        //Purpose of this function is to adjust the currently existing hard-coded points so that
        //the entire emblem is aligned symmetrically across the origin and be scallable
        
        //emblem -> array of 2D points that define spline curvature
        //scaleFactor -> enlarge or decrease size of emblem (defined by user)
        //shiftX -> hard coded for shifting first point in emblemS1 to be aligned with vertical axis
        //shiftY -> hard coded for shifting first point in emblemS1 so entire emblem is centered around the origin
        //flip -> flag to determine if coordinates need to be flipped across vertical axis
    function shiftFlipScale(context is Context, emblem is array, scaleFactor is number, shiftX is number, shiftY is number, flip is number) {
        for (var i = 0; i != 9; i += 1) {
            for (var j = 0; j != size(emblem[i]); j += 1) {
                if (flip == 1) {
                    emblem[i][j][0] *= -1;
                    emblem[i][j][1] *= 1;
                }
                emblem[i][j][0] += shiftX * millimeter;
                emblem[i][j][1] += shiftY * millimeter;
                emblem[i][j] *= scaleFactor / 40;
            }
        }
        return (emblem);
    }
    
    
    //sketchEmblem : Draws the emblem using the emblemRight and emblemLeft arrays, passed a parameter.  Also takes in a query (the selected plane or face the user
    //               selects
    
        //leftArray -> left half of emblem (tied to variable emblemLeft)
        //rightArray -> right half of emblem (tied to variable emblemRight)
        //selectedFace -> face/plane user selects for the sketch to be projected on
    function sketchEmblem(context is Context, id is Id, leftArray is array, rightArray is array, emblemSections is number, selectedFace is Query) {
            var sketch1 = newSketch(context, id + "sketch1", {
                    "sketchPlane" : selectedFace
            });
            for (var i = 0; i < emblemSections; i += 1) {
                skFitSpline(sketch1, ("splineA" ~ toString(i)), {
                            "points" : leftArray[i]
                        });
            }
            for (var i = 0; i < emblemSections; i += 1) {
                skFitSpline(sketch1, ("splineB" ~ toString(i)), {
                            "points" : rightArray[i]
                        });
            }
            skSolve(sketch1);
    }
    
    
    //extrudeEmblem : Extrudes the emblem using the sketch id from the sketchEmblem function.  Blind extrude with depth.
    
        //sketchId -> id that represents sketch from sketchEmblem function
        //selectedFace -> face/plane user selects for the sketch to be projected on
        //depth -> amt to extrude (user defined)
    function extrudeEmblem(context is Context, id is Id, sketchId is string, selectedFace is Query, depth is map) {
            opExtrude(context, id + "extrude1", {
                        "entities" : qSketchRegion(id + sketchId, true),
                        "direction" : evOwnerSketchPlane(context, { "entity" : qCreatedBy(id + sketchId, EntityType.FACE) }).normal,
                        "endBound" : BoundingType.BLIND,
                        "endDepth" : depth
                    });
            
            opDeleteBodies(context, id + "deleteBodies1", {
                "entities" : qSketchFilter(qEverything(), SketchObject.YES)
            });
    }
