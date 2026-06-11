namespace Servis_Centar_Za_Gitare.Dtos
{
    public record MyGuitarDto(long Id, string Marka, string TipGitare, string SerijskiBroj, DateTime DatumZaprimanja);

    public record MyServiceOrderDto(
        long Id,
        long GitaraId,
        string Gitara,
        string VrstaPopravka,
        string Status,
        string OpisKvara,
        DateTime DatumOtvaranja,
        DateTime? DatumZatvaranja);
}
