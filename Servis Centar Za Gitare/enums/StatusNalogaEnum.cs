using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Servis_Centar_Za_Gitare.enums
{
    public enum StatusNalogaEnum
    {
        Zaprimljen,
        [Display(Name = "U Obradi")]
        UObradi,
        [Display(Name = "Čeka Dijelove")]
        CekaDijelove,
        [Display(Name = "Završеn")]
        Zavrsen,
        [Display(Name = "Otkazan")]
        Otkazan,
    }
}
