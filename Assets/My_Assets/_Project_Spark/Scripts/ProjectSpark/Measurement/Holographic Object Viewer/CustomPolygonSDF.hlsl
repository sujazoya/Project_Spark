#ifndef PROJECT_SPARK_CUSTOM_POLYGON_SDF_INCLUDED
#define PROJECT_SPARK_CUSTOM_POLYGON_SDF_INCLUDED


/* ============================================================
   SEGMENT DISTANCE
   ============================================================ */

float ProjectSparkPolygonSegmentDistance(
    float2 p,
    float2 a,
    float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;

    float denominator =
        max(
            dot(ba, ba),
            0.000001
        );

    float h =
        saturate(
            dot(pa, ba) /
            denominator
        );

    float2 closest =
        a + ba * h;

    return length(
        p - closest
    );
}


/* ============================================================
   MINIMUM EDGE DISTANCE
   ============================================================ */

void ProjectSparkPolygonEdgeDistance(
    float2 p,
    float2 a,
    float2 b,
    inout float minimumDistance)
{
    float distanceValue =
        ProjectSparkPolygonSegmentDistance(
            p,
            a,
            b
        );

    minimumDistance =
        min(
            minimumDistance,
            distanceValue
        );
}


/* ============================================================
   ROBUST RAY CROSSING
   ============================================================

   Uses the standard half-open interval rule:

       (a.y > p.y) != (b.y > p.y)

   This prevents a polygon vertex from being counted twice.

   The ray travels toward +X.
   ============================================================ */

void ProjectSparkPolygonCrossing(
    float2 p,
    float2 a,
    float2 b,
    inout float crossingCount)
{
    float aAbove =
        step(
            p.y,
            a.y
        );

    float bAbove =
        step(
            p.y,
            b.y
        );

    /*
        XOR.

        0 = both on same side
        1 = edge crosses horizontal ray level
    */
    float differentSide =
        abs(
            aAbove -
            bAbove
        );

    /*
        Avoid horizontal edges.
    */
    float nonHorizontal =
        step(
            0.000001,
            abs(
                b.y -
                a.y
            )
        );

    float valid =
        differentSide *
        nonHorizontal;

    /*
        Exact X coordinate of intersection.
    */
    float denominator =
        b.y -
        a.y;

    /*
        Preserve the sign of the denominator.
    */
    float safeDenominator =
        denominator +
        (
            step(
                abs(denominator),
                0.000001
            )
            *
            (
                denominator >= 0.0
                    ? 0.000001
                    : -0.000001
            )
        );

    float intersectionX =
        a.x +
        (
            (p.y - a.y) *
            (b.x - a.x)
        )
        /
        safeDenominator;

    /*
        Ray goes toward +X.
    */
    float crossesRight =
        step(
            p.x,
            intersectionX
        );

    crossingCount +=
        valid *
        crossesRight;
}


/* ============================================================
   MAIN FLOAT FUNCTION
   ============================================================ */

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
        ALWAYS initialize the output.
    */
    SDF = 0.0;


    /*
        Reconstruct P7.
    */
    float2 P7 =
        float2(
            P7X,
            P7Y
        );


    /*
        Clamp vertex count.
    */
    float count =
        clamp(
            PointCount,
            3.0,
            8.0
        );


    /*
        Start with a large distance.
    */
    float minimumDistance =
        100000.0;


    /*
        Ray crossing count.
    */
    float crossingCount =
        0.0;


    /* ========================================================
       EDGE 0

       P0 -> P1
       ======================================================== */

    ProjectSparkPolygonEdgeDistance(
        P,
        P0,
        P1,
        minimumDistance
    );

    ProjectSparkPolygonCrossing(
        P,
        P0,
        P1,
        crossingCount
    );


    /* ========================================================
       EDGE 1

       P1 -> P2
       ======================================================== */

    ProjectSparkPolygonEdgeDistance(
        P,
        P1,
        P2,
        minimumDistance
    );

    ProjectSparkPolygonCrossing(
        P,
        P1,
        P2,
        crossingCount
    );


    /* ========================================================
       EDGE 2

       P2 -> P3
       ======================================================== */

    if (count >= 4.0)
    {
        ProjectSparkPolygonEdgeDistance(
            P,
            P2,
            P3,
            minimumDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P2,
            P3,
            crossingCount
        );
    }


    /* ========================================================
       EDGE 3

       P3 -> P4
       ======================================================== */

    if (count >= 5.0)
    {
        ProjectSparkPolygonEdgeDistance(
            P,
            P3,
            P4,
            minimumDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P3,
            P4,
            crossingCount
        );
    }


    /* ========================================================
       EDGE 4

       P4 -> P5
       ======================================================== */

    if (count >= 6.0)
    {
        ProjectSparkPolygonEdgeDistance(
            P,
            P4,
            P5,
            minimumDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P4,
            P5,
            crossingCount
        );
    }


    /* ========================================================
       EDGE 5

       P5 -> P6
       ======================================================== */

    if (count >= 7.0)
    {
        ProjectSparkPolygonEdgeDistance(
            P,
            P5,
            P6,
            minimumDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P5,
            P6,
            crossingCount
        );
    }


    /* ========================================================
       EDGE 6

       P6 -> P7
       ======================================================== */

    if (count >= 8.0)
    {
        ProjectSparkPolygonEdgeDistance(
            P,
            P6,
            P7,
            minimumDistance
        );

        ProjectSparkPolygonCrossing(
            P,
            P6,
            P7,
            crossingCount
        );
    }


    /* ========================================================
       CLOSING EDGE

       Triangle:
           P2 -> P0

       Quad:
           P3 -> P0

       Pentagon:
           P4 -> P0

       Hexagon:
           P5 -> P0

       Heptagon:
           P6 -> P0

       Octagon:
           P7 -> P0
       ======================================================== */

    float2 closingPoint =
        P2;


    if (count >= 4.0)
    {
        closingPoint =
            P3;
    }

    if (count >= 5.0)
    {
        closingPoint =
            P4;
    }

    if (count >= 6.0)
    {
        closingPoint =
            P5;
    }

    if (count >= 7.0)
    {
        closingPoint =
            P6;
    }

    if (count >= 8.0)
    {
        closingPoint =
            P7;
    }


    ProjectSparkPolygonEdgeDistance(
        P,
        closingPoint,
        P0,
        minimumDistance
    );

    ProjectSparkPolygonCrossing(
        P,
        closingPoint,
        P0,
        crossingCount
    );


    /* ========================================================
       EVEN / ODD TEST
       ======================================================== */

    float halfCount =
        floor(
            crossingCount *
            0.5
        );

    float evenCount =
        halfCount *
        2.0;

    float inside =
        crossingCount -
        evenCount;


    /*
        inside = 0
            outside

        inside = 1
            inside
    */

    SDF =
        lerp(
            minimumDistance,
            -minimumDistance,
            inside
        );
}


/* ============================================================
   HALF PRECISION ENTRY POINT
   ============================================================ */

void CustomPolygonSDF_half(
    half2 P,
    half2 P0,
    half2 P1,
    half2 P2,
    half2 P3,
    half2 P4,
    half2 P5,
    half2 P6,
    half P7X,
    half P7Y,
    half PointCount,
    out half SDF)
{
    float result;


    CustomPolygonSDF_float(
        (float2)P,
        (float2)P0,
        (float2)P1,
        (float2)P2,
        (float2)P3,
        (float2)P4,
        (float2)P5,
        (float2)P6,
        (float)P7X,
        (float)P7Y,
        (float)PointCount,
        result
    );


    SDF =
        (half)result;
}


#endif