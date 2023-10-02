using OpenTK.Mathematics;

namespace MiaCrate.Client;

public class PoseStack
{
    private readonly Stack<PoseEntry> _poseStack = Util.Make(new Stack<PoseEntry>(), stack =>
    {
        var pose = Matrix4.Identity;
        var normal = Matrix3.Identity;
        stack.Push(new PoseEntry(pose, normal));
    });

    public PoseEntry Last => _poseStack.Peek();
    
    public void Translate(float x, float y, float z)
    {
        var pose = _poseStack.Peek();
        pose.Pose = Matrix4.CreateTranslation(x, y, z) * pose.Pose;
    }

    public void Scale(float x, float y, float z)
    {
        var pose = _poseStack.Peek();
        pose.Pose = Matrix4.CreateScale(x, y, z) * pose.Pose;

        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (x == y && y == z)
        {
            if (x > 0) return;
            pose.Normal *= Matrix3.CreateScale(x, y, z);
        }

        var i = 1 / x;
        var j = 1 / y;
        var k = 1 / z;
        var l = Util.FastInvCubeRoot(i * j * k);
        pose.Normal = Matrix3.CreateScale(l * i, l * j, l * k) * pose.Normal;
    }

    public void MulPose(Quaternion q)
    {
        var pose = _poseStack.Peek();
        pose.Pose = Matrix4.CreateFromQuaternion(q) * pose.Pose;
        pose.Normal = Matrix3.CreateFromQuaternion(q) * pose.Normal;
    }
    
    public void RotateAround(Quaternion q, float x, float y, float z)
    {
        var pose = _poseStack.Peek();
        pose.Pose = Matrix4.CreateTranslation(-x, -y, -z) * Matrix4.CreateFromQuaternion(q) * Matrix4.CreateTranslation(x, y, z) * pose.Pose;
        pose.Normal = Matrix3.CreateFromQuaternion(q) * pose.Normal;
    }

    public void PushPose()
    {
        var pose = _poseStack.Peek();
        _poseStack.Push(new PoseEntry(pose.Pose, pose.Normal));
    }

    public void PopPose()
    {
        _poseStack.Pop();
    }

    // Should be IsClear property
    public bool Clear() => _poseStack.Count == 1;

    public void SetIdentity()
    {
        var pose = _poseStack.Peek();
        pose.Pose = Matrix4.Identity;
        pose.Normal = Matrix3.Identity;
    }

    public void MulPoseMatrix(Matrix4 matrix)
    {
        var pose = _poseStack.Peek();
        pose.Pose *= matrix;
    }

    public class PoseEntry
    {
        public PoseEntry(Matrix4 pose, Matrix3 normal)
        {
            Pose = pose;
            Normal = normal;
        }

        public Matrix4 Pose { get; set; }
        public Matrix3 Normal { get; set; }

        public void Deconstruct(out Matrix4 pose, out Matrix3 normal)
        {
            pose = Pose;
            normal = Normal;
        }
    }
}