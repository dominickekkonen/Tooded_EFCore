using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore_Mihhailenko;

public static class ArveOst
{
    public static Tellimus Salvesta(
        ToodedContext db,
        Kasutaja buyer,
        Dictionary<int, int> cart,
        List<Toode> displayedProducts)
    {
        if (cart.Count == 0)
            throw new InvalidOperationException(
                "Ostukorv on tühi.");

        using var transaction = db.Database.BeginTransaction(
            IsolationLevel.Serializable);

        var order = new Tellimus
        {
            KasutajaId = buyer.Id,
            Aeg = DateTime.Now
        };

        foreach (var item in cart)
        {
            var product = db.Toodetabel.Find(item.Key);

            var displayed = displayedProducts.FirstOrDefault(
                t => t.Id == item.Key);

            if (product == null ||
                displayed == null ||
                item.Value <= 0 ||
                product.Kogus < item.Value)
            {
                throw new InvalidOperationException(
                    "Laoseis on muutunud. Sulge arve ja " +
                    "kontrolli ostukorvi uuesti.");
            }

            if (product.Hind != displayed.Hind)
            {
                throw new InvalidOperationException(
                    "Toote hind on muutunud. Sulge arve ja " +
                    "kontrolli ostukorvi uuesti.");
            }

            product.Kogus -= item.Value;

            order.Read.Add(new TellimuseRida
            {
                ToodeId = product.Id,
                ToodeNimi = product.Toodenimetus,
                Kogus = item.Value,
                Hind = product.Hind
            });

            order.Summa += product.Hind * item.Value;
        }

        if (order.Summa > 99999999.99m)
            throw new InvalidOperationException(
                "Tellimuse summa on liiga suur.");

        db.Tellimustabel.Add(order);
        db.SaveChanges();
        transaction.Commit();

        return order;
    }
}