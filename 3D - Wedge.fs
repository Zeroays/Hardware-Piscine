FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "Triangle-Step" }

export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {   
        //Triangular step piece assumed to have equal sides
        annotation { "Name" : "sTri_Dim" }
        isLength(definition.sTri_Dim, { (millimeter) : [1, 30, 30000] } as LengthBoundSpec);
        
        annotation { "Name" : "lTri_Dim" }
        isLength(definition.lTri_Dim, { (millimeter) : [1, 50, 30000] } as LengthBoundSpec);

        annotation { "Name" : "sTri_Depth" }
        isLength(definition.sTri_Depth, { (millimeter) : [1, 30, 30000] } as LengthBoundSpec);

        annotation { "Name" : "lTri_Depth" }
        isLength(definition.lTri_Depth, { (millimeter) : [1, 50, 30000] } as LengthBoundSpec);
    }
    {
        //Defining points to draw the two triangles
        //Triangle1 and Triangle2 represent three pairs of points for which lines are drawn
        var triangle1 = [
            vector(0, 0) * millimeter, vector(0, definition.lTri_Dim / millimeter) * millimeter,
            vector(0, 0) * millimeter, vector(definition.lTri_Dim / millimeter, 0) * millimeter,
            vector(0, definition.lTri_Dim / millimeter) * millimeter, vector(definition.lTri_Dim / millimeter, 0) * millimeter
        ];
        
        var triangle2 = [
            vector(0, definition.sTri_Dim / millimeter) * millimeter, vector(definition.sTri_Dim / millimeter, definition.sTri_Dim / millimeter) * millimeter,
            vector(0, definition.sTri_Dim / millimeter) * millimeter, vector(definition.sTri_Dim / millimeter, 0) * millimeter,
            vector(definition.sTri_Dim / millimeter, 0) * millimeter, vector(definition.sTri_Dim / millimeter, definition.sTri_Dim / millimeter) * millimeter
        ];

        //draw and create each Triangle by calling their respective functions
        drawTriangle(id, context, "sketch1", "Front", triangle1);
        
        createTriangle(id, context, "sketch1", "Front", "extrude1", definition.lTri_Depth);
        
        drawTriangle(id, context, "sketch2", "Front", triangle2);
        
        createTriangle(id, context, "sketch2", "Front", "extrude2", definition.sTri_Depth);
        
        
        //mateDist represents the perpendicular bisector distance between the angled face from the smaller triangle to larger one
        var mateDist = (definition.lTri_Dim - definition.sTri_Dim)  / (2 * millimeter);
        opTransform(context, id + "transform1", {
                "bodies" : qCreatedBy(id + "extrude2", EntityType.BODY),
                "transform" : transform(vector(mateDist, 0, mateDist) * millimeter) 
        });
        
        //Join the bodies
        opBoolean(context, id + "boolean1", {
                "tools" : qAllNonMeshSolidBodies(),
                "operationType" : BooleanOperationType.UNION
        });
        
        //Delete sketches for cleanup
        opDeleteBodies(context, id + "deleteBodies1", {
                "entities" : qSketchFilter(qEverything(), SketchObject.YES)
        });   
        
    });


    //drawTriangle
        //sketch -> id of sketch
        //plane -> name of plane for which the solid model will start from
        //triangle -> the array that represents the three pairs of points from the Triangle variables
    function drawTriangle(id is Id, context is Context, sketch is string, plane is string, triangle is array) {
        var sketch1 = newSketch(context, id + sketch, {
                        "sketchPlane" : qCreatedBy(makeId(plane), EntityType.FACE)
                    });
        
        skLineSegment(sketch1, "line1", {
                    "start" : triangle[0],
                    "end" : triangle[1]
                });
        skLineSegment(sketch1, "line2", {
                    "start" : triangle[2],
                    "end" : triangle[3]
                });
    
        skLineSegment(sketch1, "line3", {
                    "start" : triangle[4],
                    "end" : triangle[5]
                });
    
        skSolve(sketch1);
    }


    //createTriangle :
        //sketch -> id of sketch
        //plane -> name of plane for which the solid model will start from
        //extrude -> name for extrude ; programmer can pass value
        //depth -> depth/length of extrusion for triangle
    function createTriangle(id is Id, context is Context, sketch is string, plane is string, extrude is string, depth is map) {
                opExtrude(context, id + extrude, {
                            "entities" : qSketchRegion(id + sketch),
                            "direction" : evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + sketch) }).normal,
                            "startBound" : BoundingType.BLIND,
                            "endBound" : BoundingType.BLIND,
                            "startDepth" : depth / 2,
                            "endDepth" : depth / 2
                        });
    }  
