using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    [Serializable]
    class KDTree
    {
        static readonly Vector3[] axes = { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
        const int maxRecursionDepth = 6;
        const int minNumTriangles = 5;
        
        private Node root;

        private enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        private struct StackElem
        {
            /// <summary>
            /// reference to far child
            /// </summary>
            public Node node;
            /// <summary>
            /// entry/exit distance
            /// </summary>
            public float t;
            /// <summary>
            /// coordinates of entry/exit point
            /// </summary>
            public Vector3 pb;
            /// <summary>
            /// previous element in stack
            /// </summary>
            public int prev;
        }

        public KDTree(TriangleMesh tm)
        {
            root = new Node(tm, Vector3.UnitX);
            buildKDTree(root, 0);
        }

        private KDTree()
        {
        }

        private void buildKDTree(Node node, int recursionDepth)
        {
            if (recursionDepth > maxRecursionDepth || node.numTriangles() < minNumTriangles)
                return;
            node.split();   // split cell and create child nodes
            buildKDTree(node.leftNode, recursionDepth + 1);
            buildKDTree(node.rightNode, recursionDepth + 1);
        }

        public void SaveToFile(string fileName)
        {
            string fullFileName = fileName;
            if (!fileName.ToLower().EndsWith(".kdt"))
                fullFileName += ".kdt";
            //BinaryWriter writer = null;
            Stream stream = null;
            try
            {
                stream = File.OpenWrite(fullFileName);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                /*writer = new BinaryWriter(new BufferedStream(stream));
                write(writer, root);*/
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        public static KDTree LoadFromFile(string fileName)
        {
            string fullFileName = fileName;
            if (!fileName.ToLower().EndsWith(".kdt"))
                fullFileName += ".kdt";
            Stream stream = null;
            KDTree kdTree = null;
            try
            {
                stream = File.OpenRead(fullFileName);
                BinaryFormatter formatter = new BinaryFormatter();
                kdTree = (KDTree)formatter.Deserialize(stream);
                /*BinaryReader reader = new BinaryReader(new BufferedStream(stream));
                kdTree.root = read(reader);*/
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return kdTree;
        }

        /*static void write(BinaryWriter writer, Node node)
        {
            writer.Write(node.SplittingPlaneD);
            writer.Write(node.SplittingPlaneNormal);

            writer.Write(node.Triangles.Length);
            foreach (Triangle tri in node)
            {
                foreach (float f in vectorToArray(tri.vertex1))
                    writer.Write(f);
                foreach (float f in vectorToArray(tri.vertex2))
                    writer.Write(f);
                foreach (float f in vectorToArray(tri.vertex3))
                    writer.Write(f);
            }

            foreach (float f in node.BoundingBox)
                writer.Write(f);

            if (node.isLeaf())
                return;
            write(writer, node.leftNode);
            write(writer, node.rightNode);
        }

        static Node read(BinaryReader reader)
        {
            Node node = new Node();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                node.SplittingPlaneD = reader.ReadSingle();
                node.SplittingPlaneNormal = reader.ReadByte();

                int nrTriangles = reader.ReadInt32();
                for (int i = 0; i < nrTriangles; i++)
                {
                    Vector3 v1 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 v2 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 v3 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    node.addTriangle(new Triangle(v1, v2, v3));
                }

                float[] boundingBoxMinMax = new float[6];
                for (int i = 0; i < boundingBoxMinMax.Length; i++)
                {
                    boundingBoxMinMax[i] = reader.ReadSingle();
                }
            }

            node.leftNode = read(reader);
            node.rightNode = read(reader);

            return node;
        }*/

        public Triangle? RayTraversal(Ray ray, float? maxTracingDistance)
        {
            return RayTravRECB(root, ray, maxTracingDistance);
        }

        private Triangle? RayTravRECB(Node node, Ray ray, float? maxTracingDistance)
        {
            float a = 0;    // entry distance
            float b = 0;    // exit distance
            float t;    // distance to the splitting plane

            if (!node.rayBoxIntersect(ray, ref a, ref b))
                return null;

            float distanceTraced = a;

            StackElem[] stack = new StackElem[50];
            Node farChild;
            Node currentNode = root;
            int enPt = 0;
            stack[enPt].t = a;
            if (a > 0) // a ray with external origin
                stack[enPt].pb = ray.Position + a * ray.Direction;
            else   // a ray with internal origin
                stack[enPt].pb = ray.Position;

            int exPt = 1;
            stack[exPt].t = b;
            stack[exPt].pb = ray.Position + b * ray.Direction;
            stack[exPt].node = null;    // termination flag

            // traverse through the whole tree until an object is intersected or the ray leaves the scene
            while (currentNode != null)
            {
                // loop until a leaf is found
                while (!currentNode.isLeaf())
                {
                    if (maxTracingDistance.HasValue && distanceTraced > maxTracingDistance)
                        return null;    // early termination since no intersection within tracing range

                    float splitPos = currentNode.splittingPlane.D;

                    Axis axis;
                    if (currentNode.splittingPlane.Normal.Equals(Vector3.UnitX))
                        axis = Axis.X;
                    else if (currentNode.splittingPlane.Normal.Equals(Vector3.UnitY))
                        axis = Axis.Y;
                    else
                        axis = Axis.Z;

                    if (getAxis(stack[enPt].pb, axis) <= splitPos)
                    {
                        if (getAxis(stack[exPt].pb, axis) <= splitPos)
                        {
                            currentNode = currentNode.leftNode;
                            continue;
                        }
                        if (getAxis(stack[exPt].pb, axis) == splitPos)
                        {
                            currentNode = currentNode.rightNode;
                            continue;
                        }
                        farChild = currentNode.rightNode;
                        currentNode = currentNode.leftNode;
                    }
                    else
                    {
                        if (splitPos < getAxis(stack[exPt].pb, axis))
                        {
                            currentNode = currentNode.rightNode;
                            continue;
                        }
                        farChild = currentNode.leftNode;
                        currentNode = currentNode.rightNode;
                    }

                    t = (splitPos - getAxis(ray.Position, axis)) / getAxis(ray.Direction, axis);

                    distanceTraced += t;

                    // setup the new exit point
                    int temp = exPt;
                    exPt++;
                    if (exPt == enPt)
                        exPt++; // skip so as not to overwrite data

                    stack[exPt].prev = temp;
                    stack[exPt].t = t;
                    stack[exPt].node = farChild;
                    setAxis(ref stack[exPt].pb, axis, splitPos);
                    Axis nextAxis = getNextAxis(axis);
                    setAxis(ref stack[exPt].pb, nextAxis, 
                        getAxis(ray.Position, nextAxis) + t * getAxis(ray.Direction, nextAxis));
                    Axis prevAxis = getPreviousAxis(axis);
                    setAxis(ref stack[exPt].pb, prevAxis, 
                        getAxis(ray.Position, prevAxis) + t * getAxis(ray.Direction, prevAxis));
                }

                float closestIntersection = float.MaxValue;
                Triangle? closestTriangle = null;

                foreach (Triangle tri in currentNode)
                {
                    /*if (currentNode.getRightExtreme(tri) - currentNode.splittingPlane.D < stack[enPt].t ||
                        currentNode.getLeftExtreme(tri) - currentNode.splittingPlane.D > stack[exPt].t)
                        continue;*/
                    float? dist = Intersection.test(ray, tri);
                    if (dist.HasValue && dist.Value < closestIntersection)
                    {
                        closestIntersection = dist.Value;
                        closestTriangle = tri;
                    }
                }

                if (closestTriangle.HasValue)
                    return closestTriangle;

                enPt = exPt;
                currentNode = stack[exPt].node;
                exPt = stack[enPt].prev;
            }

            return null;
        }

        #region Helper methods

        private static float[] vectorToArray(Vector3 vector)
        {
            return new float[] { vector.X, vector.Y, vector.Z };
        }

        private static Axis getNextAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Axis.Y;
                case Axis.Y:
                    return Axis.Z;
                case Axis.Z:
                    return Axis.X;
                default:
                    return 0;
            }
        }
        
        private static Axis getPreviousAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Axis.Z;
                case Axis.Y:
                    return Axis.X;
                case Axis.Z:
                    return Axis.Y;
                default:
                    return 0;
            }
        }

        private static void setAxis(ref Vector3 vector, Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X:
                    vector.X = value;
                    break;
                case Axis.Y:
                    vector.Y = value;
                    break;
                case Axis.Z:
                    vector.Z = value;
                    break;
            }
        }

        private static float getAxis(Vector3 vector, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return vector.X;
                case Axis.Y:
                    return vector.Y;
                case Axis.Z:
                    return vector.Z;
                default:
                    return 0;
            }
        }

        #endregion

        [Serializable]
        private class Node : IEnumerable<Triangle>
        {
            #region fields

            [NonSerialized]
            private List<Triangle> triangles = new List<Triangle>();
            [NonSerialized]
            internal Plane splittingPlane;
            [NonSerialized]
            internal BoundingBox boundingBox;

            internal Node leftNode, rightNode;

            #endregion
            
            #region properties

            internal Triangle[] Triangles
            {
                get { return triangles.ToArray(); }
                set
                {
                    if (value == null)
                        return;
                    triangles.Clear();
                    foreach (Triangle tri in value)
                        triangles.Add(tri);
                }
            }

            internal Vector3 SplittingPlane
            {
                get { return splittingPlane.D * splittingPlane.Normal; }
                set
                {
                    splittingPlane = new Plane(value, value.Length());
                    splittingPlane.Normalize();
                }
            }

            internal float[] BoundingBox
            {
                get
                {
                    float[] minMax = { boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z,
                                     boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z };
                    return minMax;
                }
                set
                {
                    boundingBox.Min = new Vector3(value[0], value[1], value[2]);
                    boundingBox.Max = new Vector3(value[3], value[4], value[5]);
                }
            }

            #endregion

            #region constructors

            public Node(TriangleMesh tm, Vector3 axis)
            {
                splittingPlane = new Plane(axis, 0);
                boundingBox = tm.getBoundingBox();
                triangles.AddRange(tm.Triangles);
            }

            private Node(Vector3 axis, BoundingBox box)
            {
                splittingPlane = new Plane(axis, 0);
                boundingBox = box;
            }

            #endregion

            internal bool isLeaf()
            {
                return leftNode == null && rightNode == null;
            }

            internal void addTriangle(Triangle tri)
            {
                if (isLeaf())
                    triangles.Add(tri);
            }

            internal int numTriangles()
            {
                return triangles.Count;
            }

            internal bool intersectsLeft(Triangle tri)
            {
                return getLeftExtreme(tri) <= splittingPlane.D;
            }

            internal bool intersectsRight(Triangle tri)
            {
                return getRightExtreme(tri) >= splittingPlane.D;
            }

            internal void split()
            {
                float bestPos = 0;
                float bestCost = float.MaxValue;

                foreach (Triangle tri in this)
                {
                    float leftExtreme = getLeftExtreme(tri);
                    float rightExtreme = getRightExtreme(tri);
                    float cost = calculateCost(leftExtreme);
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestPos = leftExtreme;
                    }
                    cost = calculateCost(rightExtreme);
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestPos = rightExtreme;
                    }
                }
                splittingPlane.D = bestPos;

                Vector3 nextAxis;
                if (splittingPlane.Normal.Equals(Vector3.UnitX))
                    nextAxis = Vector3.UnitY;
                else if (splittingPlane.Normal.Equals(Vector3.UnitY))
                    nextAxis = Vector3.UnitZ;
                else
                    nextAxis = Vector3.UnitX;

                Vector3 maxDec =
                    (splittingPlane.DotNormal(boundingBox.Max) - splittingPlane.D) * splittingPlane.Normal;
                leftNode = new Node(nextAxis,
                    new BoundingBox(boundingBox.Min, boundingBox.Max - maxDec));
                Vector3 minInc =
                    (splittingPlane.D - splittingPlane.DotNormal(boundingBox.Min)) * splittingPlane.Normal;
                rightNode = new Node(nextAxis,
                    new BoundingBox(boundingBox.Min + minInc, boundingBox.Max));

                foreach (Triangle tri in this)
                {
                    if (intersectsLeft(tri))
                        leftNode.triangles.Add(tri); ;
                    if (intersectsRight(tri))
                        rightNode.triangles.Add(tri);
                }
                triangles.Clear();
            }

            internal float calculateCost(float splitPos)
            {
                float leftArea = calculateLeftArea(splitPos);
                float rightArea = calculateRightArea(splitPos);
                float leftCount = calculateLeftTriangleCount(splitPos);
                float rightCount = calculateRightTriangleCount(splitPos);
                return 0.3f + 1.0f * (leftArea * leftCount + rightArea * rightCount);
            }

            internal float calculateLeftTriangleCount(float splitPos)
            {
                int count = 0;
                foreach (Triangle tri in this)
                    if (getLeftExtreme(tri) < splitPos)
                        count++;
                return count;
            }

            internal float calculateRightTriangleCount(float splitPos)
            {
                int count = 0;
                foreach (Triangle tri in this)
                    if (getRightExtreme(tri) > splitPos)
                        count++;
                return count;
            }

            internal float calculateLeftArea(float splitPos)
            {
                Vector3 diagonal = boundingBox.Max - boundingBox.Min;
                float[] whd = new float[3]; // width/depth/height
                for (int i = 0; i < axes.Length; i++)
                {
                    Vector3 v = axes[i];
                    if (v.Equals(splittingPlane.Normal))
                        whd[i] = Math.Abs(splitPos - Vector3.Dot(boundingBox.Min, v));
                    else
                        whd[i] = Math.Abs(Vector3.Dot(diagonal, v));
                }
                return 2 * (whd[0] * whd[1] * whd[2]);
            }

            internal float calculateRightArea(float splitPos)
            {
                Vector3 diagonal = boundingBox.Max - boundingBox.Min;
                float[] whd = new float[3]; // width/depth/height
                for (int i = 0; i < axes.Length; i++)
                {
                    Vector3 v = axes[i];
                    if (v.Equals(splittingPlane.Normal))
                        whd[i] = Math.Abs(Vector3.Dot(boundingBox.Max, v) - splitPos);
                    else
                        whd[i] = Math.Abs(Vector3.Dot(diagonal, v));
                }
                return 2 * (whd[0] * whd[1] * whd[2]);
            }

            internal float getLeftExtreme(Triangle tri)
            {
                float leftExtreme = splittingPlane.DotNormal(tri.vertex1);
                float next = splittingPlane.DotNormal(tri.vertex2);
                if (next < leftExtreme)
                    leftExtreme = next;
                next = splittingPlane.DotNormal(tri.vertex3);
                if (next < leftExtreme)
                    leftExtreme = next;
                return leftExtreme;
            }

            internal float getRightExtreme(Triangle tri)
            {
                float rightExtreme = splittingPlane.DotNormal(tri.vertex1);
                float next = splittingPlane.DotNormal(tri.vertex2);
                if (next > rightExtreme)
                    rightExtreme = next;
                next = splittingPlane.DotNormal(tri.vertex3);
                if (next > rightExtreme)
                    rightExtreme = next;
                return rightExtreme;
            }

            internal bool rayBoxIntersect(Ray ray, ref float entryDist, ref float exitDist)
            {
                float? dist = ray.Intersects(boundingBox);
                if (!dist.HasValue)
                    return false;
                entryDist = dist.Value;
                Vector3 pos = ray.Position +
                    (dist.Value + (boundingBox.Max - boundingBox.Min).LengthSquared() + 1) * ray.Direction;
                Ray oppositeRay = new Ray(pos, -ray.Direction);
                float? oppositeDist = oppositeRay.Intersects(boundingBox);
                if (!oppositeDist.HasValue) // might happen due to floating point precision errors
                    return false;
                exitDist = (pos - ray.Position).Length() - oppositeDist.Value;
                return true;
            }

            #region IEnumerable<Triangle> Members

            public IEnumerator<Triangle> GetEnumerator()
            {
                return triangles.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
    }
}