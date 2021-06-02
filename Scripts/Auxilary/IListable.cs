public interface IListable
{
    /// <summary>
    /// using for representing a list of crews,expeditions and artifacts
    /// </summary>
    int GetItemsCount();
    bool HaveSelectedObject();
    string GetName(int index);
    int GetID(int index);
}