using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Servis_Centar_Za_Gitare.enums
{
    public enum VrstaPopravkaEnum
    {
        [Display(Name = "Zamjena Žica")]
        ZamjenaZica,
        [Display(Name = "Podešavanje Vrata")]
        PodesavanjeVrata,
        [Display(Name = "Podešavanje Intonacije")]
        PodesavanjeIntonacije,
        [Display(Name = "Zamjena Pragova")]
        ZamjenaPragova,
        [Display(Name = "Brušenje Pragova")]
        BrusenjePragova,
        [Display(Name = "Popravak Elektronike")]
        PopravakElektronike,
        [Display(Name = "Zamjena Pickupa")]
        ZamjenaPickupa,
        [Display(Name = "Zamjena Mašinica")]
        ZamjenaMasinica,
        [Display(Name = "Popravak Kobilice")]
        PopravakKobilice,
        [Display(Name = "Čišćenje Potenciometara")]
        CiscenjePotenciometara
    }
}
