#ifndef PROJECT_SPARK_CUSTOM_POLYGON_SDF_INCLUDED
#define PROJECT_SPARK_CUSTOM_POLYGON_SDF_INCLUDED


float ProjectSparkPolygonSegmentDistance(
    float2 p,
    float2 a,
    float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;

    float denominator = max(
        dot(ba, ba),
        0.000001
    );

    float h = saturate(
        dot(pa, ba) / denominator
    );

    float2 closest = a + ba * h;

    return length(p - closest);
}


void ProjectSparkPolygonEdge(
    float2 p,
    float2 a,
    float2 b,
    float minDistanceIn,
    out float minDistanceOut)
{
    float distanceValue =
        ProjectSparkPolygonSegmentDistance(
            p,
            a,
            b
        );

    minDistanceOut =
        min(
            minDistanceIn,
            distanceValue
        );
}


/*
    General 2D polygon point-in-polygon test.

    Uses ray crossing.

    Works for:
        - convex polygons
        - concave polygons
        - triangles
        - quadrilaterals
        - pentagons
        - hexagons
        - heptagons
        - octagons

    No arrays.
    No dynamic indexing.
    No modulo.
    No bool output.
*/
void ProjectSparkPolygonCrossing(
    float2 p,
    float2 a,
    float2 b,
    float insideIn,
    out float insideOut)
{
    float ay = a.y;
    float by = b.y;

    float crossesVerticalRange =
        step(
            min(ay, by),
            p.y
        ) *
        step(
            p.y,
            max(ay, by)
        );

    float differentY =
        step(
            0.000001,
            abs(by - ay)
        );

    float validRange =
        crossesVerticalRange *
        differentY;

    float intersectionX =
        a.x +
        (p.y - ay) *
        (b.x - a.x) /
        max(
            by - ay,
            0.000001
        );

    float crossesRight =
        step(
            p.x,
            intersectionX
        );

    /*
        Toggle using a fractional state.

        For each crossing:
            0 -> 1
            1 -> 0
    */
    float toggle =
        validRange *
        crossesRight;

    insideOut =
        abs(
            insideIn - toggle
        );
}


/*
    Main Project Spark polygon SDF.

    P:
        Centered UV coordinate.

    P0-P7:
        Polygon vertices in perimeter order.

    PointCount:
        Number of active vertices, 3-8.

    SDF:
        Negative = inside
         0      = boundary
        Positive = outside
*/
void CustomPolygonSDF_float(
    float2 P,
    float2 P0,
    float2 P1,
    float2 P2,
    float2 P3,
    float2 P4,
    float2 P5,
    float2 P6,
    float P7X,
    float P7Y,
    float PointCount,
    out float SDF)
{
    /*
        IMPORTANT:

        Shader Graph / D3D11 requires
        the output to be initialized.
    */
    SDF = 0.0;


    /*
        Reconstruct P7.

        This avoids passing a Vector2 P7 through
        some Shader Graph versions that generate
        problematic preview code.
    */
    float2 P7 =
        float2(
            P7X,
            P7Y
        );


    /*
        Clamp polygon vertex count.
    */
    float count =
        clamp(
            PointCount,
            3.0,
            8.0
        );


    /*
        Start with a very large distance.
    */
    float minDistance =
        100000.0;


    /*
        Point-in-polygon state.

        0 = outside
        1 = inside
    */
    float inside =
        0.0;


    /*
        ------------------------------------------------
        EDGE 0
        P0 -> P1
        ------------------------------------------------
    */

    if (count >= 3.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P0,
            P1,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P0,
            P1,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 1
        P1 -> P2
        ------------------------------------------------
    */

    if (count >= 3.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P1,
            P2,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P1,
            P2,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 2
        P2 -> P3
        ------------------------------------------------
    */

    if (count >= 4.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P2,
            P3,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P2,
            P3,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 3
        P3 -> P4
        ------------------------------------------------
    */

    if (count >= 5.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P3,
            P4,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P3,
            P4,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 4
        P4 -> P5
        ------------------------------------------------
    */

    if (count >= 6.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P4,
            P5,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P4,
            P5,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 5
        P5 -> P6
        ------------------------------------------------
    */

    if (count >= 7.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P5,
            P6,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P5,
            P6,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        EDGE 6
        P6 -> P7
        ------------------------------------------------
    */

    if (count >= 8.0)
    {
        ProjectSparkPolygonEdge(
            P,
            P6,
            P7,
            minDistance,
            minDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P6,
            P7,
            inside,
            inside
        );
    }


    /*
        ------------------------------------------------
        CLOSING EDGE
        ------------------------------------------------

        Triangle:
            P2 -> P0

        Quad:
            P3 -> P0

        Pentagon:
            P4 -> P0

        ...

        Octagon:
            P7 -> P0
    */

    float2 closingPoint =
        P2;

    if (count >= 4.0)
    {
        closingPoint = P3;
    }

    if (count >= 5.0)
    {
        closingPoint = P4;
    }

    if (count >= 6.0)
    {
        closingPoint = P5;
    }

    if (count >= 7.0)
    {
        closingPoint = P6;
    }

    if (count >= 8.0)
    {
        closingPoint = P7;
    }


    ProjectSparkPolygonEdge(
        P,
        closingPoint,
        P0,
        minDistance,
        minDistance
    );


    ProjectSparkPolygonCrossing(
        P,
        closingPoint,
        P0,
        inside,
        inside
    );


    /*
        ------------------------------------------------
        FINAL SIGN
        ------------------------------------------------
    */

    float insideMask =
        step(
            0.5,
            inside
        );


    /*
        Negative inside.
        Positive outside.
    */
    SDF =
        lerp(
            minDistance,
            -minDistance,
            insideMask
        );
}


#endif