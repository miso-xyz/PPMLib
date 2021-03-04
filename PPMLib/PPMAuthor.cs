namespace PPMLib
{
    public class PPMAuthor
    {
        public PPMAuthor(string name, ulong id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public ulong Id { get; }

        public override string ToString()
            => $"{Name} ({Id.ToString("X8")})";
    }
}
