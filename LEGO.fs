FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "LEGO" }
export const myCube = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        //User input from GUI
       annotation { "Name" : "Rows" }
       isInteger(definition.row, { (unitless) : [1, 2, 100] } as IntegerBoundSpec);
       
       annotation { "Name" : "Columns" }
       isInteger(definition.column, { (unitless) : [1, 2, 100] } as IntegerBoundSpec);
       
       annotation { "Name" : "Depth" }
       isLength(definition.depth, { (millimeter) : [2, 9.6, 1000] } as LengthBoundSpec);
    }
    {
        //Predefined variables for a single LEGO unit
        //Can change depending on passed annotation values
        var uLength = 8 * millimeter;
        var uWidth = 8 * millimeter;
        var uStudDist = 4 * millimeter;
        var uStudDia = 4.8 * millimeter;
        var uStudHeight = 1.6 * millimeter;
        var shellAmt = 1.6 * millimeter;
        var mateInnerDia = 4.8 * millimeter;
        var mateOuterDia = 6.4 * millimeter;
        var postDia = 3.2 * millimeter;
        
        //Extrusions of Lego Body, Studs, and Text
        legoBody(id, context, "lego-w-studs", uLength * definition.row, uWidth * definition.column, definition.depth);
        legoStuds(id, context, "lws", "lego-w-studs", uLength * definition.row, uWidth * definition.column, uStudDia, uStudDist, uStudHeight);
        
        //Embed Logo on Stud
        studLogo(id, context, "logo", "lego-w-studs", uStudDist, uStudDia, uStudHeight, uWidth * definition.column, uLength * definition.row);
        
        //Shell bottom of LEGO piece
        opShell(context, id + "shell1", {
                "entities" : qNthElement(qCreatedBy(id + "lego-w-studs", EntityType.FACE), 1),
                "thickness" : -shellAmt
        });
        
        
        //Conditional statement for a 2 x 2 piece.  Should have cylinder/mate on bottom instead of post
        if (definition.row >= 2 && definition.column >= 2)
            createMates(id, context, mateInnerDia, mateOuterDia, uLength * definition.row, uWidth * definition.column, uLength, uWidth, definition.depth - shellAmt);
            
        //Creates Posts (conditional is defined within the function itself)
        createPosts(id, context, uLength * definition.row, uWidth * definition.column, postDia, uWidth, uStudDist, definition.depth - shellAmt, definition.row, definition.column);
    
    
        //Deletes all sketches.  Cleanup
        opDeleteBodies(context, id + "deleteBodies1", {
                "entities" : qSketchFilter(qEverything(), SketchObject.YES)
            });
      
    });
    
    //All functions below take in the standard id and context as part of Featurescript's data handling
    
    
    
    //legoBody : Creates the N x M LEGO piece (without studs, logo, post/cylinder, etc) where N is the amount of columns, and M is the amounts of rows, specified 
    //           by user in annotation
    
        //cubeId -> User assigned id for the main body
        //length -> Length of LEGO piece (can change uLength to vary)
        //width -> Width of LEGO piece (can change uWidth to vary)
        //depth -> Depth of LEGO piece (varies by user's definition.depth)
    function legoBody(id is Id, context is Context, cubeId is string, length is map, width is map, depth is map) {
        fCuboid(context, id + cubeId, {
               "corner1" : vector(0, 0, 0) * millimeter,
               "corner2" : vector(1, 0, 0) * width + vector(0, 1, 0) * length + vector(0, 0, 1) * depth
       });      
    }
    
    
    //legoStuds : Draws and extrudes each stud and patterns them around the main lego piece, created by the above legoBody function.  A unique id is assigned to
    //           each stud sketch and extrusion
    
        //sketchId -> User assigned id for stud sketch
        //cubeId -> User assigned id for main LEGO body (which should be the same cubeId as the one passed into the legoBody function)
        //length -> Length of LEGO piece (can change uLength to vary)
        //width -> Width of LEGO piece (can change uWidth to vary)
        //studDia -> Diameter of stud ; Can Change to radius by dividing by 2 (can change ustudDia to vary)
        //studDist -> Distance from bottom left corner of main body to stud (can change uStudDist to vary)
        //studHeight -> Stud Height (can change uStudHeight to vary)
    function legoStuds(id is Id, context is Context, sketchId is string, cubeId is string, length is map, width is map, studDia is map, studDist is map, studHeight is map) {
          var sketch1 = newSketch(context, id + sketchId, {
                  "sketchPlane" : qNthElement(qCreatedBy(id + cubeId, EntityType.FACE), 2)
          });
          var studStart = vector(1, 0) * studDist + vector(0, 1) * studDist;
          for (var i = 0; 2 * i * studDist < width; i += 1) {
              for (var j = 0; 2 * j * studDist < length; j += 1) {
                  skCircle(sketch1, "circle1a."~i~'.'~j, {
                          "center" : studStart + vector(1, 0) * studDist * 2 * i + vector(0, 1) * studDist * 2 * j,
                          "radius" : studDia / 2
                  });
                  
              }
          }
          skSolve(sketch1);   
        extrudeAndJoin(id, context, "lws", "extrude1", "boolean1", studHeight);
    }
    
    
    //studLogo : Draws and extrudes the stud logo for each stud.  Similar to legoStuds function above, but uses skText built-in function in FS to generate text.
    //           Can change the displayed text by changing the "text" parameter inside the skText function.
    
        //sketchId -> User assigned id for stud sketch
        //cubeId -> User assigned id for main LEGO body (which should be the same cubeId as the one passed into the legoBody function)
        //studDia -> Diameter of stud ; Can Change to radius by dividing by 2 (can change ustudDia to vary)
        //studDist -> Distance from bottom left corner of main body to stud, assumed equal both horizontally and vertically (can change uStudDist to vary)
        //studHeight -> Stud Height (can change uStudHeight to vary)
        //length -> Length of LEGO piece (can change uLength to vary)
        //width -> Width of LEGO piece (can change uWidth to vary)
    function studLogo(id is Id, context is Context, sketchId is string, cubeId is string, studDist is map, studDia is map, studHeight is map, width is map, length is map) {
        var sketchLogo = newSketch(context, id + sketchId, {
                "sketchPlane" : qNthElement(qCreatedBy(id + cubeId, EntityType.FACE), 5)
        });
        var logoDepth = 1.6 * millimeter;
        var studStart = vector(1, 0) * studDist + vector(0, 1) * studDist;
        var legoTopLeft = vector(1, 0) * studDia / (2.25);
        var legoBottomRight = vector(0, 1) * studDia / (8);
        for (var i = 0; 2 * i * studDist < width; i += 1) {
              for (var j = 0; 2 * j * studDist < length; j += 1) {
                skText(sketchLogo, "lego."~i~'.'~j, {
                        "text" : "LEGO",
                        "fontName" : "OpenSans-Regular.ttf",
                        "firstCorner" : studStart + (vector(1, 0) * studDist * 2 * i - legoTopLeft) + (vector(0, 1) * studDist * 2 * j + legoBottomRight),
                        "secondCorner" : studStart + (vector(1, 0) * studDist * 2 * i + legoTopLeft) + (vector(0, 1) * studDist * 2 * j - legoBottomRight)
                });
              }
        }
        skSolve(sketchLogo);
        opExtrude(context, id + "logoext", {
                          "entities" : qSketchRegion(id + "logo", true), 
                          "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "logo")}).normal,
                          "startBound" : BoundingType.BLIND,
                        "endBound" : BoundingType.BLIND,
                        "startDepth" : 0,
                        "endDepth" : logoDepth + studHeight / 4
        });
    }
    
    //createMates : Creates the mates (aka cylinders) that act as the snap-on structure for the LEGO studs.  This function is only called by createPosts when a 2 x M piece needs to be
    //              drawn.  M is an integer >= 2.  
    
        //inDia -> Inner Diameter of cylinder/mate (can change mateInnerDia to vary)
        //outDia -> Outer Diameter of cylinder/mate (can change materOuterDia to vary)
        //mateHeight -> Distance from bottom left corner of main body to cylinder, vertically (can change uLength to vary)
        //mateWidth -> Distance from bottom left corner of main body to cylinder, horizontally (can change uWidth to vary)
    function createMates(id is Id, context is Context, inDia is map, outDia is map, length is map, width is map, mateHeight is map, mateWidth is map, extrudeAmt is map) {
        var sketch2 = newSketch(context, id + "sketch2", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        // Create sketch entities here
        for (var i = 1; i * mateWidth < width; i += 1) {
            for (var j = 1; j * mateHeight < length; j += 1) {
                skCircle(sketch2, "mateInner."~i~'.'~j, {
                        "center" : vector(1, 0) * mateWidth * i + vector(0, 1) * mateHeight * j,
                        "radius" : inDia / 2
                });
                skCircle(sketch2, "mateOuter."~i~'.'~j, {
                        "center" : vector(1, 0) * mateWidth * i + vector(0, 1) * mateHeight * j,
                        "radius" : outDia / 2
                });
            }
        }
        skSolve(sketch2);
        extrudeAndJoin(id, context, "sketch2", "extrude2", "boolean2", extrudeAmt);
    }
    
    
    
    //createPosts : Creates the posts that are generated should the LEGO piece not fit the criteria of the createMates function.  In this case, posts are generated for 1 x M pieces,
    //              where M is an integer >= 2
        
        //length -> Length of LEGO piece (can change uLength to vary)
        //width -> Width of LEGO piece (can change uWidth to vary)
        //postDia -> Diameter of Post (can change postDia to vary)
        //postWidth -> Distance from bottom left corner of Lego piece to post, horizontally (can change uWidth to vary)
        //postHeight -> Distance from bottom left corner of Lego piece to post, vertically (can change uLength to vary)
        //extrudeAmt -> Amt to extrude from main body's bottom surface up to next (formula ; see called function in main loop)
        //row -> Amt of rows for Lego mainbody (user defined)
        //columns -> Amt of cols for Lego mainbody (user defined)
    function createPosts(id is Id, context is Context, length is map, width is map, postDia is map, postWidth is map, postHeight is map, extrudeAmt is map, row is number, column is number) {
        if (row == 1 && column >= 2) {
             var sketch3 = newSketch(context, id + "sketch3", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            for (var i = 1; i * postWidth < width; i += 1) {
                skCircle(sketch3, "post."~i, {
                            "center" : vector(1, 0) * postWidth * i + vector(0, 1) * postHeight,
                            "radius" : postDia / 2
                });
            }
            skSolve(sketch3);
            extrudeAndJoin(id, context, "sketch3", "extrude3", "boolean3", extrudeAmt);
        } else if (column == 1 && row >= 2) {
             var sketch4 = newSketch(context, id + "sketch4", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            for (var i = 1; i * postHeight < length / 2; i += 1) {
                skCircle(sketch4, "post."~i, {
                            "center" : vector(1, 0) * postHeight + vector(0, 1) * postHeight * 2 * i,
                            "radius" : postDia / 2
                });
            }
            skSolve(sketch4);
            extrudeAndJoin(id, context, "sketch4", "extrude4", "boolean4", extrudeAmt);
        }       
    }
    
    //extrudeAndJoin : Extrudes sketches pre-defined in all of the above functions.  In addition, extrudes (which are treated as 
    //                 separate parts inside the Onshape UI interface) are then joined using the opBoolean function
    
        //sketchId -> User assigned id for stud sketch
        //boolId -> User assigned id for boolean (joining parts)
        //extrudeAmt -> Amount to extrude sketch
    
    function extrudeAndJoin(id is Id, context is Context, sketchId is string, extrudeId is string, boolId is string, extrudeAmt is map) {
        opExtrude(context, id + extrudeId, {
              "entities" : qSketchRegion(id + sketchId, true), 
              "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + sketchId)}).normal,
              "endBound" : BoundingType.BLIND,
              "endDepth" : extrudeAmt
        });
        opBoolean(context, id + boolId, {
            "tools" : qAllNonMeshSolidBodies(),
            "operationType" : BooleanOperationType.UNION
        });
    }
    

