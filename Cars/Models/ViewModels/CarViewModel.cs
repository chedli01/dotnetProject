namespace Cars.Models.ViewModels
{
    public class CarViewModel
    {
        public Car Car { get; set; }
        public IEnumerable<Make> Makes { get; set; }
        public IEnumerable<Model> Models { get; set; }
        public IEnumerable<Currency> Currencies { get; set; }

        private List<Currency> CList = new List<Currency>();

        private List<Currency> CreateList()
        {
            CList.Add(new Currency("USD", "USD"));
            CList.Add(new Currency("INR", "INR"));
            CList.Add(new Currency("DT", "DT"));
            return CList;
        }

        public CarViewModel()
        {
            Currencies = this.CreateList();
        }

    }
    public class Currency
    {
        public String Id { get; set; }
        public String Name { get; set; }

        public Currency(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
