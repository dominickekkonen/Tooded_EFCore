namespace Tooded_EFCore;

public static class AktiivneKasutaja
{
    public static Kasutaja? Praegune { get; private set; }

    public static bool OnAdmin =>
        KasutajaOigused.OnAdmin(Praegune);

    public static void LogiSisse(Kasutaja user) =>
        Praegune = user;

    public static void LogiValja() =>
        Praegune = null;
}