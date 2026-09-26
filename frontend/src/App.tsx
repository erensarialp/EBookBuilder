import {
  useEffect,
  useState,
} from "react";

import {
  AlertCircle,
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  Info,
} from "lucide-react";

import { DragDropProvider } from "@dnd-kit/react";

import {
  isSortable,
} from "@dnd-kit/react/sortable";

import FileDropzone from "./components/FileDropzone";
import SortableFileItem from "./components/SortableFileItem";
import GenerationProgress from "./components/GenerationProgress";
import PdfResult from "./components/PdfResult";

import {
  createBook,
  getPdfUrl,
} from "./services/bookService";

import type {
  UploadedDocument,
} from "./types/document";

import type {
  BookResponse,
} from "./types/book";

type PageStatus =
  | "idle"
  | "generating"
  | "success";

function App() {
  const [bookName, setBookName] =
    useState("");

  const [documents, setDocuments] =
    useState<UploadedDocument[]>([]);

  const [error, setError] =
    useState("");

  const [status, setStatus] =
    useState<PageStatus>("idle");

  const [
    progressStep,
    setProgressStep,
  ] = useState(0);

  const [
    createdBook,
    setCreatedBook,
  ] =
    useState<BookResponse | null>(
      null,
    );

  useEffect(() => {
    if (status !== "generating") {
      return;
    }

    const interval =
      window.setInterval(() => {
        setProgressStep(
          (currentStep) => {
            if (currentStep >= 3) {
              return currentStep;
            }

            return currentStep + 1;
          },
        );
      }, 900);

    return () => {
      window.clearInterval(
        interval,
      );
    };
  }, [status]);

  const handleFilesSelected = (
    incomingFiles: File[],
  ) => {
    setError("");

    const docxFiles =
      incomingFiles.filter(
        (file) =>
          file.name
            .toLowerCase()
            .endsWith(".docx"),
      );

    if (
      docxFiles.length !==
      incomingFiles.length
    ) {
      setError(
        "Yalnızca .docx uzantılı Word dosyaları yükleyebilirsiniz.",
      );
    }

    if (docxFiles.length === 0) {
      return;
    }

    const uniqueFiles =
      docxFiles.filter(
        (file) =>
          !documents.some(
            (existing) =>
              existing.name ===
                file.name &&
              existing.size ===
                file.size,
          ),
      );

    if (
      uniqueFiles.length !==
      docxFiles.length
    ) {
      setError(
        "Aynı dosya birden fazla kez eklenemez.",
      );
    }

    const availableSlots =
      10 - documents.length;

    if (availableSlots <= 0) {
      setError(
        "En fazla 10 adet Word dosyası yükleyebilirsiniz.",
      );

      return;
    }

    if (
      uniqueFiles.length >
      availableSlots
    ) {
      setError(
        `Yalnızca ${availableSlots} dosya daha ekleyebilirsiniz.`,
      );
    }

    const filesToAdd =
      uniqueFiles.slice(
        0,
        availableSlots,
      );

    const mappedFiles:
      UploadedDocument[] =
      filesToAdd.map(
        (file) => ({
          id: crypto.randomUUID(),
          file,
          name: file.name,
          size: file.size,
        }),
      );

    setDocuments(
      (current) => [
        ...current,
        ...mappedFiles,
      ],
    );
  };

  const handleRemoveDocument = (
    id: string,
  ) => {
    setDocuments(
      (current) =>
        current.filter(
          (document) =>
            document.id !== id,
        ),
    );

    setError("");
  };

  const handleDragEnd = (
    event: Parameters<
      NonNullable<
        React.ComponentProps<
          typeof DragDropProvider
        >["onDragEnd"]
      >
    >[0],
  ) => {
    if (event.canceled) {
      return;
    }

    const { source } =
      event.operation;

    if (!isSortable(source)) {
      return;
    }

    const {
      initialIndex,
      index,
    } = source;

    if (
      initialIndex === index
    ) {
      return;
    }

    setDocuments(
      (current) => {
        const reordered = [
          ...current,
        ];

        const [movedItem] =
          reordered.splice(
            initialIndex,
            1,
          );

        reordered.splice(
          index,
          0,
          movedItem,
        );

        return reordered;
      },
    );
  };

  const canGenerate =
    bookName.trim().length > 0 &&
    documents.length === 10;

  const missingDocumentCount =
    Math.max(
      0,
      10 - documents.length,
    );

  const getRequirementMessage =
    () => {
      const isBookNameMissing =
        bookName.trim().length === 0;

      if (
        isBookNameMissing &&
        missingDocumentCount > 0
      ) {
        return `Kitap adını girin ve ${missingDocumentCount} dosya daha yükleyin.`;
      }

      if (isBookNameMissing) {
        return "Kitap adını girin.";
      }

      if (
        missingDocumentCount > 0
      ) {
        return `${missingDocumentCount} dosya daha yüklemelisiniz.`;
      }

      return "";
    };

  const handleGenerate =
    async () => {
      setError("");

      if (!bookName.trim()) {
        setError(
          "Lütfen kitap adını girin.",
        );

        return;
      }

      if (
        documents.length !== 10
      ) {
        setError(
          "Kitap oluşturmak için tam olarak 10 Word dosyası yüklemelisiniz.",
        );

        return;
      }

      setProgressStep(0);
      setStatus("generating");

      try {
        const response =
          await createBook(
            bookName,
            documents,
          );

        setProgressStep(4);

        await new Promise(
          (resolve) =>
            setTimeout(
              resolve,
              500,
            ),
        );

        setCreatedBook(
          response,
        );

        setStatus("success");
      } catch (exception) {
        setStatus("idle");
        setProgressStep(0);

        if (
          exception instanceof Error
        ) {
          setError(
            exception.message,
          );
        } else {
          setError(
            "E-kitap oluşturulurken beklenmeyen bir hata oluştu.",
          );
        }
      }
    };

  const handleCreateNewBook =
    () => {
      setBookName("");
      setDocuments([]);
      setError("");
      setProgressStep(0);
      setCreatedBook(null);
      setStatus("idle");
    };

  if (
    status ===
    "generating"
  ) {
    return (
      <main
        className="
          min-h-screen
          bg-[#F7F8FA]
          px-4
          py-10
          sm:px-6
          lg:px-8
        "
      >
        <Header />

        <div className="mt-10">
          <GenerationProgress
            activeStep={
              progressStep
            }
          />
        </div>
      </main>
    );
  }

  if (
    status === "success" &&
    createdBook
  ) {
    const pdfUrl =
      createdBook.pdfPath
        ? getPdfUrl(
            createdBook.pdfPath,
          )
        : undefined;

    return (
      <main
        className="
          min-h-screen
          bg-[#F7F8FA]
          px-4
          py-8
          sm:px-6
          lg:px-8
        "
      >
        <div
          className="
            mx-auto
            max-w-5xl
          "
        >
          <Header />

          <div className="mt-8">
            <PdfResult
              bookName={
                createdBook.name
              }
              documentCount={
                createdBook
                  .papers.length
              }
              pdfUrl={
                pdfUrl
              }
              onCreateNew={
                handleCreateNewBook
              }
            />
          </div>
        </div>
      </main>
    );
  }

  return (
    <main
      className="
        min-h-screen
        bg-[#F7F8FA]
        px-4
        py-8
        sm:px-6
        lg:px-8
      "
    >
      <div
        className="
          mx-auto
          max-w-3xl
        "
      >
        <Header />

        <div
          className="
            mt-8
            overflow-hidden
            rounded-3xl
            border
            border-[#E5E7EB]
            bg-white
            shadow-sm
          "
        >
          <section
            className="
              border-b
              border-[#E5E7EB]
              p-5
              sm:p-7
            "
          >
            <SectionTitle
              number={1}
              title="Kitap Bilgileri"
            />

            <div className="mt-5">
              <label
                htmlFor="bookName"
                className="
                  mb-2
                  block
                  text-sm
                  font-medium
                  text-[#374151]
                "
              >
                Kitap Adı{" "}

                <span className="text-red-500">
                  *
                </span>
              </label>

              <input
                id="bookName"
                type="text"
                required
                value={
                  bookName
                }
                onChange={(
                  event,
                ) => {
                  setBookName(
                    event.target.value,
                  );

                  setError("");
                }}
                placeholder="Örn. Sürdürülebilir Kentler Bildiriler Kitabı 2026"
                className="
                  w-full
                  rounded-xl
                  border
                  border-[#D1D5DB]
                  bg-white
                  px-4
                  py-3
                  text-sm
                  text-[#1F2937]
                  outline-none
                  transition
                  placeholder:text-[#9CA3AF]
                  focus:border-[#2563EB]
                  focus:ring-4
                  focus:ring-blue-100
                "
              />
            </div>
          </section>

          <section
            className="
              border-b
              border-[#E5E7EB]
              p-5
              sm:p-7
            "
          >
            <SectionTitle
              number={2}
              title="Dosyaları Yükle"
            />

            <div className="mt-5">
              <FileDropzone
                fileCount={
                  documents.length
                }
                onFilesSelected={
                  handleFilesSelected
                }
              />
            </div>
          </section>

          <section
            className="
              border-b
              border-[#E5E7EB]
              p-5
              sm:p-7
            "
          >
            <div
              className="
                flex
                items-center
                justify-between
                gap-4
              "
            >
              <SectionTitle
                number={3}
                title="Yüklenen Dosyalar"
              />

              <span
                className={`
                  text-sm
                  font-bold
                  ${
                    documents.length ===
                    10
                      ? "text-[#16A34A]"
                      : "text-[#6B7280]"
                  }
                `}
              >
                {
                  documents.length
                }{" "}
                / 10
              </span>
            </div>

            {documents.length ===
            0 ? (
              <div
                className="
                  mt-5
                  flex
                  gap-3
                  rounded-xl
                  bg-blue-50
                  p-4
                  text-sm
                  text-[#4B5563]
                "
              >
                <Info
                  size={20}
                  className="
                    shrink-0
                    text-[#2563EB]
                  "
                />

                <div>
                  <p className="font-medium">
                    Henüz dosya yüklenmedi.
                  </p>

                  <p
                    className="
                      mt-1
                      text-xs
                      text-[#6B7280]
                    "
                  >
                    Lütfen 10 adet Word dosyası seçin.
                  </p>
                </div>
              </div>
            ) : (
              <>
                <div
                  className="
                    mt-5
                    rounded-lg
                    bg-[#F9FAFB]
                    px-3
                    py-2
                    text-xs
                    text-[#6B7280]
                  "
                >
                  Dosyaların sırasını
                  değiştirmek için sol
                  taraftaki tutamacı
                  sürükleyin.
                </div>

                <DragDropProvider
                  onDragEnd={
                    handleDragEnd
                  }
                >
                  <div
                    className="
                      mt-3
                      space-y-2
                    "
                  >
                    {documents.map(
                      (
                        document,
                        index,
                      ) => (
                        <SortableFileItem
                          key={
                            document.id
                          }
                          item={
                            document
                          }
                          index={
                            index
                          }
                          onRemove={
                            handleRemoveDocument
                          }
                        />
                      ),
                    )}
                  </div>
                </DragDropProvider>
              </>
            )}
          </section>

          {error && (
            <div
              className="
                mx-5
                mt-5
                flex
                gap-3
                rounded-xl
                border
                border-red-200
                bg-red-50
                p-4
                sm:mx-7
              "
            >
              <AlertCircle
                size={20}
                className="
                  shrink-0
                  text-[#DC2626]
                "
              />

              <p
                className="
                  text-sm
                  text-red-700
                "
              >
                {error}
              </p>
            </div>
          )}

          <section
            className="
              p-5
              sm:p-7
            "
          >
            {canGenerate && (
              <div
                className="
                  mb-4
                  flex
                  items-center
                  gap-2
                  rounded-xl
                  border
                  border-green-200
                  bg-green-50
                  px-4
                  py-3.5
                  text-sm
                  text-green-700
                "
              >
                <CheckCircle2
                  size={20}
                  className="shrink-0"
                />

                <div>
                  <p className="font-semibold">
                    Her şey hazır
                  </p>

                  <p className="mt-0.5">
                    E-kitabınızı artık
                    oluşturabilirsiniz.
                  </p>
                </div>
              </div>
            )}

            {!canGenerate && (
              <div
                className="
                  mb-4
                  flex
                  items-start
                  gap-3
                  rounded-xl
                  border
                  border-amber-200
                  bg-amber-50
                  px-4
                  py-3.5
                "
              >
                <AlertTriangle
                  size={20}
                  className="
                    mt-0.5
                    shrink-0
                    text-amber-600
                  "
                />

                <div>
                  <p
                    className="
                      text-sm
                      font-semibold
                      text-amber-900
                    "
                  >
                    Eksik bilgi var
                  </p>

                  <p
                    className="
                      mt-0.5
                      text-sm
                      text-amber-700
                    "
                  >
                    {
                      getRequirementMessage()
                    }
                  </p>
                </div>
              </div>
            )}

            <button
              type="button"
              disabled={
                !canGenerate
              }
              onClick={
                handleGenerate
              }
              className={`
                flex
                w-full
                items-center
                justify-center
                gap-2
                rounded-xl
                px-5
                py-3.5
                text-sm
                font-semibold
                transition-all

                ${
                  canGenerate
                    ? `
                      bg-[#2563EB]
                      text-white
                      shadow-sm
                      hover:bg-blue-700
                      hover:shadow-md
                    `
                    : `
                      cursor-not-allowed
                      bg-[#E5E7EB]
                      text-[#9CA3AF]
                    `
                }
              `}
            >
              <BookOpen
                size={19}
              />

              Kitabı Oluştur
            </button>
          </section>
        </div>

        <footer
          className="
            mt-6
            text-center
            text-xs
            text-[#9CA3AF]
          "
        >
          Eren Sarıalp · 2026
        </footer>
      </div>
    </main>
  );
}

function Header() {
  return (
    <header
      className="
        mx-auto
        max-w-3xl
      "
    >
      <div
        className="
          flex
          items-center
          gap-3
        "
      >
        <div
          className="
            flex
            h-11
            w-11
            items-center
            justify-center
            rounded-xl
            bg-[#2563EB]
            text-white
            shadow-sm
          "
        >
          <BookOpen
            size={23}
          />
        </div>

        <div>
          <h1
            className="
              text-xl
              font-bold
              tracking-tight
              text-[#111827]
              sm:text-2xl
            "
          >
            E-Kitap Oluşturucu
          </h1>

          <p
            className="
              mt-0.5
              text-xs
              text-[#6B7280]
              sm:text-sm
            "
          >
            Word dosyalarınızı
            tek bir düzenli PDF
            e-kitaba dönüştürün.
          </p>
        </div>
      </div>
    </header>
  );
}

interface SectionTitleProps {
  number: number;
  title: string;
}

function SectionTitle({
  number,
  title,
}: SectionTitleProps) {
  return (
    <div
      className="
        flex
        items-center
        gap-3
      "
    >
      <div
        className="
          flex
          h-7
          w-7
          shrink-0
          items-center
          justify-center
          rounded-full
          bg-[#2563EB]
          text-xs
          font-bold
          text-white
        "
      >
        {number}
      </div>

      <h2
        className="
          text-base
          font-semibold
          text-[#1F2937]
        "
      >
        {title}
      </h2>
    </div>
  );
}

export default App;