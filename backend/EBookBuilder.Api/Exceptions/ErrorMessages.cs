namespace EBookBuilder.Api.Exceptions;

public static class ErrorMessages
{
    public const string BookNameRequired =
        "Kitap adı zorunludur.";

    public const string ExactlyTenDocumentsRequired =
        "Tam olarak 10 adet .docx dosyası yüklemelisiniz.";

    public const string InvalidDocumentType =
        "Yalnızca .docx uzantılı Word dosyaları yüklenebilir.";

    public const string EmptyDocument =
        "Boş bir Word dosyası yüklenemez.";

    public const string BookNotFound =
        "Kitap bulunamadı.";

    public const string UnexpectedError =
        "İşlem sırasında beklenmeyen bir hata oluştu.";
}