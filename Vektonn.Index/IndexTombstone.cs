namespace Vektonn.Index
{
    public record IndexTombstone<TId>(TId Id)
        where TId : notnull;
}
