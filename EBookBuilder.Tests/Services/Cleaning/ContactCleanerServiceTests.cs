using EBookBuilder.Api.Services.Cleaning;

namespace EBookBuilder.Tests.Services.Cleaning;

public class ContactCleanerServiceTests
{
    private readonly ContactCleanerService _service;

    public ContactCleanerServiceTests()
    {
        _service = new ContactCleanerService();
    }

    [Fact]
    public void CleanText_ShouldRemoveEmailAndPhone_ButKeepOrcid()
    {
        var input =
            "E-posta: elif.kaya@example.org | " +
            "Tel: 0500 000 00 01 | " +
            "ORCID: 0000-0001-1000-0001";

        var result = _service.CleanText(input);

        Assert.Equal(
            "ORCID: 0000-0001-1000-0001",
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveEnglishEmailAndInternationalPhone()
    {
        var input =
            "Email: mert.demir@example.org | " +
            "Telefon: +90 (500) 000 00 02 | " +
            "ORCID: 0000-0001-1000-0002";

        var result = _service.CleanText(input);

        Assert.Equal(
            "ORCID: 0000-0001-1000-0002",
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveEmailAndGsmCompletely()
    {
        var input =
            "E-posta derya.akin@example.org / " +
            "GSM 0 (500) 000 00 12";

        var result = _service.CleanText(input);

        Assert.Equal(
            string.Empty,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldNotModifyNormalText()
    {
        var input =
            "Anadolu Örnek Enstitüsü, Bilgisayar Bilimleri Bölümü";

        var result = _service.CleanText(input);

        Assert.Equal(
            input,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldNotRemoveOrcid()
    {
        var input =
            "ORCID: 0000-0001-1000-0001";

        var result = _service.CleanText(input);

        Assert.Equal(
            input,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveIncompleteTelephoneLabel()
    {
        var input = "Telefon: (";

        var result = _service.CleanText(input);

        Assert.Equal(
            string.Empty,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveMobileLabel()
    {
        var input = "Mobile";

        var result = _service.CleanText(input);

        Assert.Equal(
            string.Empty,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveTelNoLabel()
    {
        var input = "Tel.No";

        var result = _service.CleanText(input);

        Assert.Equal(
            string.Empty,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveMailAndContactLabels()
    {
        var input = "Mail: | İrtibat";

        var result = _service.CleanText(input);

        Assert.Equal(
            string.Empty,
            result
        );
    }

    [Fact]
    public void CleanText_ShouldRemoveContactPrefix_ButKeepOrcid()
    {
        var input =
            "İletişim: - - ORCID 0000-0001-1000-0003";

        var result = _service.CleanText(input);

        Assert.Equal(
            "ORCID 0000-0001-1000-0003",
            result
        );
    }
}