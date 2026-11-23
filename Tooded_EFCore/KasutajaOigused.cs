using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public static class KasutajaOigused
{
    public static bool OnAdmin(Kasutaja? user) =>
        user != null &&
        user.Roll == "admin" &&
        string.Equals(
            user.Kasutajanimi,
            "admin",
            StringComparison.OrdinalIgnoreCase);

    public static void LooAdmin(ToodedContext db)
    {
        using var transaction = db.Database.BeginTransaction();

        var users = db.Kasutajatabel.ToList();

        var admin = users.SingleOrDefault(k =>
            string.Equals(
                k.Kasutajanimi.Trim(),
                "admin",
                StringComparison.OrdinalIgnoreCase));

        if (admin == null)
        {
            admin = new Kasutaja
            {
                Kasutajanimi = "admin",
                Eesnimi = "Admin",
                Perenimi = "",
                Epost = "",
                Roll = "admin"
            };

            db.Kasutajatabel.Add(admin);
        }

        admin.Kasutajanimi = "admin";
        admin.Roll = "admin";

        // Use your existing password hashing helper.
        if (string.IsNullOrWhiteSpace(admin.ParoolHash) ||
            !Salasona.Kontrolli(
                "123456", admin.ParoolHash, admin.Sool))
        {
            Salasona.Loo(
                "123456",
                out string hash,
                out string salt);

            admin.ParoolHash = hash;
            admin.Sool = salt;
        }

        // Every other account is a basic user.
        foreach (var user in users)
        {
            if (!ReferenceEquals(user, admin))
                user.Roll = "kasutaja";
        }

        db.SaveChanges();
        transaction.Commit();
    }
}