namespace InvoicePdf.Api.Invoices;

/// <summary>
/// Stand-in for a real data source. A wholesale order carries dozens of SKUs,
/// which is exactly why the PDF has to survive spanning several pages.
/// </summary>
public static class InvoiceStore
{
    private static readonly (string Sku, string Description, int Qty, decimal Price, decimal Discount)[] Catalogue =
    [
        ("BV-1001", "Cold Brew Concentrate 1L", 48, 6.40m, 0.05m),
        ("BV-1002", "Single Origin Beans - Ethiopia 1kg", 24, 18.90m, 0.10m),
        ("BV-1003", "Single Origin Beans - Colombia 1kg", 24, 16.50m, 0.10m),
        ("BV-1004", "Espresso Blend 1kg", 60, 14.20m, 0.12m),
        ("BV-1005", "Decaf Blend 1kg", 18, 15.80m, 0.00m),
        ("BV-1006", "Matcha Ceremonial Grade 100g", 36, 22.00m, 0.05m),
        ("BV-1007", "Chai Concentrate 1L", 42, 7.10m, 0.05m),
        ("BV-1008", "Oat Milk Barista 1L", 144, 2.35m, 0.15m),
        ("BV-1009", "Almond Milk Barista 1L", 96, 2.80m, 0.15m),
        ("BV-1010", "Soy Milk Barista 1L", 72, 2.15m, 0.10m),
        ("BV-1011", "Vanilla Syrup 750ml", 30, 5.60m, 0.00m),
        ("BV-1012", "Caramel Syrup 750ml", 30, 5.60m, 0.00m),
        ("BV-1013", "Hazelnut Syrup 750ml", 24, 5.60m, 0.00m),
        ("BV-1014", "Sugar Free Vanilla Syrup 750ml", 18, 6.20m, 0.00m),
        ("PK-2001", "Takeaway Cup 8oz (sleeve of 50)", 80, 4.10m, 0.08m),
        ("PK-2002", "Takeaway Cup 12oz (sleeve of 50)", 120, 4.75m, 0.08m),
        ("PK-2003", "Takeaway Cup 16oz (sleeve of 50)", 90, 5.30m, 0.08m),
        ("PK-2004", "Cup Lid 8oz (sleeve of 100)", 60, 3.90m, 0.05m),
        ("PK-2005", "Cup Lid 12/16oz (sleeve of 100)", 110, 4.20m, 0.05m),
        ("PK-2006", "Kraft Cup Sleeve (box of 200)", 45, 8.60m, 0.05m),
        ("PK-2007", "Paper Straw 8mm (box of 500)", 38, 6.90m, 0.00m),
        ("PK-2008", "Napkin 2-ply (box of 1000)", 26, 11.40m, 0.00m),
        ("PK-2009", "Takeaway Bag Medium (box of 250)", 32, 14.80m, 0.06m),
        ("PK-2010", "Carry Tray 4-cup (box of 100)", 28, 16.20m, 0.06m),
        ("FD-3001", "Butter Croissant (frozen, box of 48)", 20, 28.50m, 0.10m),
        ("FD-3002", "Pain au Chocolat (frozen, box of 48)", 18, 31.00m, 0.10m),
        ("FD-3003", "Almond Croissant (frozen, box of 36)", 12, 34.20m, 0.10m),
        ("FD-3004", "Sourdough Loaf (frozen, box of 20)", 16, 42.00m, 0.05m),
        ("FD-3005", "Banana Bread Slice (box of 24)", 22, 19.80m, 0.05m),
        ("FD-3006", "Blueberry Muffin (box of 24)", 24, 21.60m, 0.05m),
        ("FD-3007", "Vegan Brownie (box of 24)", 20, 23.40m, 0.05m),
        ("FD-3008", "Granola Pot 180g (case of 12)", 30, 18.00m, 0.00m),
        ("FD-3009", "Overnight Oats 200g (case of 12)", 26, 17.40m, 0.00m),
        ("FD-3010", "Protein Ball Twin Pack (box of 30)", 18, 26.70m, 0.00m),
        ("EQ-4001", "Portafilter Cleaning Tablets (tub of 100)", 14, 24.50m, 0.00m),
        ("EQ-4002", "Group Head Brush", 25, 4.80m, 0.00m),
        ("EQ-4003", "Milk Jug 600ml Stainless", 16, 12.90m, 0.05m),
        ("EQ-4004", "Milk Jug 950ml Stainless", 12, 15.40m, 0.05m),
        ("EQ-4005", "Tamper 58mm", 8, 29.00m, 0.00m),
        ("EQ-4006", "Knock Box Compact", 6, 34.50m, 0.00m),
        ("EQ-4007", "Digital Scale 2kg 0.1g", 10, 41.00m, 0.05m),
        ("EQ-4008", "Water Filter Cartridge", 15, 38.75m, 0.05m),
        ("EQ-4009", "Descaling Solution 1L", 20, 9.60m, 0.00m),
        ("EQ-4010", "Barista Cloth (pack of 10)", 34, 7.30m, 0.00m),
        ("EQ-4011", "Thermometer Probe", 9, 11.20m, 0.00m),
        ("EQ-4012", "Shot Glass 60ml (pack of 6)", 11, 8.90m, 0.00m),
    ];

    public static Invoice GetSample(string number = "INV-2026-0841")
    {
        var seller = new Party(
            "Northwind Coffee Supply Ltd",
            "Unit 14, Waverley Trade Park",
            "Ashfield Road",
            "Manchester",
            "M15 4QT",
            "United Kingdom",
            "GB 412 8837 05");

        var buyer = new Party(
            "Riverside Roasters Retail Ltd",
            "88 Bridgewater Street",
            "Castlefield",
            "Manchester",
            "M3 4NB",
            "United Kingdom",
            "GB 771 2094 63");

        var lines = Catalogue
            .Select(c => new InvoiceLine(c.Sku, c.Description, c.Qty, c.Price, c.Discount))
            .ToArray();

        var issued = new DateOnly(2026, 8, 5);

        return new Invoice(
            Number: number,
            IssuedOn: issued,
            DueOn: issued.AddDays(30),
            CurrencyCode: "GBP",
            TaxRate: 0.20m,
            Seller: seller,
            Buyer: buyer,
            Lines: lines,
            PaymentTerms: "Net 30. Late payments accrue interest at 8% above the Bank of England base rate.");
    }
}
