using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Circle
{
    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
}

public struct Wall
{
    public Vector2 start;
    public Vector2 end;

    public Wall(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}

public class FluidCollider : MonoBehaviour,IFluidCollider
{
    private List<Circle> list;
    private List<Wall> walls;
    public ColliderType Type => ColliderType.OTHER;
    public Vector2 center { get; private set; }

    List<Vector2> edgePoints;
    List<List<Vector2>> clusters;

    private void Start()
    {
        list = new List<Circle>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        edgePoints = ExtractSpritePoints(spriteRenderer.sprite);
        clusters = ClusterPoints(edgePoints, 6);
        list = CreateCirclesFromClusters(clusters,transform.position);
        list = SortCirclesUsingConvexHull(list);
        walls = CreateWallBetweenCircles(list);
    }

    private void Update()
    {
        
    }

    public void ResolveCollision(ref FluidParticle particle, float particleRadius, float collisionDamping)
    {

        Vector2 particlePosition = particle.position;
        Vector2 particleVelocity = particle.velocity;

        foreach (var wall in walls)
        {
            Vector2 closestPoint = GetClosestPointOnSegment(wall.start, wall.end, particlePosition);
            float distanceToWall = Vector2.Distance(particlePosition, closestPoint);

            if (distanceToWall < particleRadius)
            {
                Vector2 dir = particlePosition - closestPoint;
                dir.Normalize();

                Vector2 newPosition = closestPoint + dir * particleRadius;
                Vector2 newVelocity = particleVelocity - 2 * Vector2.Dot(particleVelocity, dir) * dir * collisionDamping;

                particle.UpdatePosition(newPosition);
                particle.UpdateVelocity(newVelocity.x, newVelocity.y);
            }
        }
    }

    List<Vector2> ExtractSpritePoints(Sprite sprite)
    {
        List<Vector2> edgePoints = new List<Vector2>();

        Vector2[] vertices = sprite.vertices;
        ushort[] triangles = sprite.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            edgePoints.Add(vertices[triangles[i]]);
            edgePoints.Add(vertices[triangles[i + 1]]);
            edgePoints.Add(vertices[triangles[i + 2]]);
        }

        return edgePoints;
    }

    public List<List<Vector2>> ClusterPoints(List<Vector2> points, int numClusters)
    {
        List<List<Vector2>> clusters = new List<List<Vector2>>();
        List<Vector2> clusterCenters = new List<Vector2>();
        System.Random random = new System.Random();

        for (int i = 0; i < numClusters; i++)
        {
            clusterCenters.Add(points[random.Next(points.Count)]);
        }

        bool changed;
        do
        {
            changed = false;
            clusters.Clear();

            for (int i = 0; i < numClusters; i++)
            {
                clusters.Add(new List<Vector2>());
            }

            foreach (var point in points)
            {
                int closestClusterCenter = FindClosestCenter(point, clusterCenters);
                clusters[closestClusterCenter].Add(point);
            }

            for (int i = 0; i < numClusters; i++)
            {
                Vector2 newClusterCenter = CalculateClusterCenter(clusters[i]);
                if (clusterCenters[i] != newClusterCenter)
                {
                    clusterCenters[i] = newClusterCenter;
                    changed = true;
                }
            }
        } while (changed);

        return clusters;
    }

    int FindClosestCenter(Vector2 point, List<Vector2> clusterCenters)
    {
        int index = 0;
        float dist = Vector2.Distance(point, clusterCenters[index]);

        for (int i = 1; i < clusterCenters.Count; i++)
        {
            float nextDist = Vector2.Distance(point, clusterCenters[i]);
            if (nextDist < dist)
            {
                dist = nextDist;
                index = i;
            }
        }

        return index;
    }

    Vector2 CalculateClusterCenter(List<Vector2> cluster)
    {
        if (cluster.Count == 0) return Vector2.zero;

        Vector2 center = Vector2.zero;
        foreach (var point in cluster)
        {
            center += point;
        }

        return center / cluster.Count;
    }

    public List<Circle> CreateCirclesFromClusters(List<List<Vector2>> clusters, Vector2 parentOffset)
    {
        List<Circle> circles = new List<Circle>();

        foreach (var cluster in clusters)
        {
            if (cluster.Count == 0) continue;

            Vector2 center = CalculateClusterCenter(cluster);
            float radius = CalculateClusterRadius(cluster, center);

            circles.Add(new Circle(center + parentOffset, radius));
        }

        return circles;
    }

    float CalculateClusterRadius(List<Vector2> cluster, Vector2 center)
    {
        float radius = 0;
        foreach (var point in cluster)
        {
            float dist = Vector2.Distance(center, point);
            if (dist > radius)
            {
                radius = dist;
            }
        }

        return radius;
    }

    List<Wall> CreateWallBetweenCircles(List<Circle> circles)
    {
        List<Wall> wallSegments = new List<Wall>();

        for (int i = 0; i < circles.Count; i++)
        {
            Vector2 start = circles[i].center;
            Vector2 end = circles[(i + 1) % circles.Count].center; // Connect to the next circle (loop around)

            wallSegments.Add(new Wall(start, end));
        }

        return wallSegments;
    }

    Vector2 GetClosestPointOnSegment(Vector2 start, Vector2 end, Vector2 point)
    {
        Vector2 segment = end - start;
        float segmentLength = segment.magnitude;
        segment.Normalize();

        float projection = Vector2.Dot(point - start, segment);
        projection = Mathf.Clamp(projection, 0, segmentLength);

        return start + segment * projection;
    }

    List<Circle> SortCirclesUsingConvexHull(List<Circle> circles)
    {
        if (circles.Count < 3)
        {
            return circles; // Convex hull is not defined for fewer than 3 points
        }

        List<Circle> hull = new List<Circle>();

        // Find the leftmost point
        Circle start = circles[0];
        foreach (Circle circle in circles)
        {
            if (circle.center.x < start.center.x)
            {
                start = circle;
            }
        }

        Circle current = start;
        do
        {
            hull.Add(current);
            Circle next = circles[0];

            foreach (Circle circle in circles)
            {
                if (next.center == current.center)
                {
                    next = circle;
                    continue;
                }

                float crossProduct = Vector3.Cross(next.center - current.center, circle.center - current.center).z;
                if (crossProduct < 0 || (crossProduct == 0 && Vector2.Distance(current.center, circle.center) > Vector2.Distance(current.center, next.center)))
                {
                    next = circle;
                }
            }

            current = next;
        } while (current.center != start.center);

        return hull;
    }

    private void OnDrawGizmos()
    {
        foreach (var item in list)
        {
            Gizmos.DrawSphere(item.center, 0.2f);
        }

        foreach (var item in walls)
        {
            Gizmos.DrawLine(item.start,item.end);
        }
    }
}

