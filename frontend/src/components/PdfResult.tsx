import {
  CheckCircle2,
  Download,
  FileText,
  Plus,
} from "lucide-react";

interface PdfResultProps {
  bookName: string;
  documentCount: number;
  pdfUrl?: string;
  onCreateNew: () => void;
}

function PdfResult({
  bookName,
  documentCount,
  pdfUrl,
  onCreateNew,
}: PdfResultProps) {
  return (
    <div className="space-y-6">
      {/* SUCCESS CARD */}

      <div
        className="
          rounded-2xl
          border
          border-green-200
          bg-green-50
          p-5
          sm:p-6
        "
      >
        <div
          className="
            flex
            flex-col
            gap-5
            sm:flex-row
            sm:items-center
            sm:justify-between
          "
        >
          <div className="flex gap-4">
            <div
              className="
                flex
                h-11
                w-11
                shrink-0
                items-center
                justify-center
                rounded-full
                bg-[#16A34A]
                text-white
              "
            >
              <CheckCircle2 size={23} />
            </div>

            <div>
              <h2
                className="
                  text-lg
                  font-bold
                  text-[#1F2937]
                "
              >
                E-kitabınız başarıyla oluşturuldu!
              </h2>

              <p
                className="
                  mt-1
                  text-sm
                  font-medium
                  text-[#374151]
                "
              >
                {bookName}
              </p>

              <p
                className="
                  mt-1
                  text-xs
                  text-[#6B7280]
                "
              >
                {documentCount} bildiri • PDF
              </p>
            </div>
          </div>

          <div
            className="
              flex
              flex-col
              gap-2
              sm:flex-row
            "
          >
            {pdfUrl ? (
              <a
                href={pdfUrl}
                download={`${bookName}.pdf`}
                className="
                  flex
                  items-center
                  justify-center
                  gap-2
                  rounded-lg
                  bg-[#2563EB]
                  px-4
                  py-2.5
                  text-sm
                  font-medium
                  text-white
                  hover:bg-blue-700
                "
              >
                <Download size={17} />

                PDF'i İndir
              </a>
            ) : (
              <button
                disabled
                type="button"
                className="
                  flex
                  items-center
                  justify-center
                  gap-2
                  rounded-lg
                  bg-blue-300
                  px-4
                  py-2.5
                  text-sm
                  font-medium
                  text-white
                "
              >
                <Download size={17} />

                PDF'i İndir
              </button>
            )}

            <button
              type="button"
              onClick={onCreateNew}
              className="
                flex
                items-center
                justify-center
                gap-2
                rounded-lg
                border
                border-[#D1D5DB]
                bg-white
                px-4
                py-2.5
                text-sm
                font-medium
                text-[#374151]
                hover:bg-gray-50
              "
            >
              <Plus size={17} />

              Yeni Kitap
            </button>
          </div>
        </div>
      </div>

      {/* PDF PREVIEW */}

      <div>
        <h3
          className="
            mb-3
            text-base
            font-semibold
            text-[#1F2937]
          "
        >
          PDF Önizleme
        </h3>

        <div
          className="
            overflow-hidden
            rounded-2xl
            border
            border-[#E5E7EB]
            bg-white
            shadow-sm
          "
        >
          {pdfUrl ? (
            <iframe
              src={pdfUrl}
              title="PDF Preview"
              className="h-[700px] w-full"
            />
          ) : (
            <div
              className="
                flex
                min-h-[500px]
                flex-col
                items-center
                justify-center
                bg-[#1F2937]
                px-6
                text-center
              "
            >
              <div
                className="
                  flex
                  h-16
                  w-16
                  items-center
                  justify-center
                  rounded-2xl
                  bg-white/10
                  text-white
                "
              >
                <FileText size={32} />
              </div>

              <h4
                className="
                  mt-5
                  text-lg
                  font-semibold
                  text-white
                "
              >
                PDF görüntüleyici hazır
              </h4>

              <p
                className="
                  mt-2
                  max-w-md
                  text-sm
                  leading-relaxed
                  text-gray-300
                "
              >
                ASP.NET Core backend bağlantısını
                yaptığımızda oluşturulan PDF burada
                görüntülenecek.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default PdfResult;