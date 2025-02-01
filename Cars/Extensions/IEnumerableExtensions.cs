using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cars.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> Items)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem sli = new SelectListItem
            {
                Text = "----Select----",
                Value = "0",
            };
            list.Add(sli);
            foreach (var item in Items)
            {
                SelectListItem sl = new SelectListItem
                {
                    Text = item.GetPropertyValue("Name"),
                    Value = item.GetPropertyValue("Id")
                };
                list.Add(sl);
            }
            return list;
        }
    }
}
