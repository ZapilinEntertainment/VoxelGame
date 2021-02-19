public abstract class MyObject
{
    virtual protected bool IsEqualNoCheck(object obj)
    {
        return true;
    }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        return IsEqualNoCheck(obj);
    }
    public static bool operator ==(MyObject A, MyObject B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(MyObject A, MyObject B)
    {
        return !(A == B);
    }
}
