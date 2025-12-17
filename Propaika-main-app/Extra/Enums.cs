using System.ComponentModel.DataAnnotations;

namespace Propaika_main_app.Extra
{
    public enum DeviceType
    {
        Phone = 0,
        Tablet = 1,
        Laptop = 2,
        Watch = 3
    }

    public enum RequestStatus
    {
        [Display(Name = "Новая")]
        New = 0,

        [Display(Name = "В процессе")]
        InProgress = 1,

        [Display(Name = "Готово")]
        Done = 2
    }
}
