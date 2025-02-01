using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cars.Models.ViewModels
{
    public class ModelViewModel
    {
        public Model Model { get; set; }
        public IEnumerable<Make> Makes { get; set; }

    }
}
