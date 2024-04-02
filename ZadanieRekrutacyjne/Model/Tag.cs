namespace ZadanieRekrutacyjne.Model
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }
        public List<object> Collectives { get; set; }
    }    
}
