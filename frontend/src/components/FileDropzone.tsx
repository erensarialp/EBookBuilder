import { useRef, useState } from "react";
import { FileUp, Plus } from "lucide-react";

interface FileDropzoneProps {
  fileCount: number;
  onFilesSelected: (files: File[]) => void;
}

function FileDropzone({
  fileCount,
  onFilesSelected,
}: FileDropzoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  const [isDragging, setIsDragging] = useState(false);

  const isFull = fileCount >= 10;

  const openFileDialog = () => {
    if (isFull) {
      return;
    }

    inputRef.current?.click();
  };

  const handleFileInput = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const files = Array.from(event.target.files ?? []);

    if (files.length > 0) {
      onFilesSelected(files);
    }

    // Aynı dosya silindikten sonra tekrar seçilebilsin.
    event.target.value = "";
  };

  const handleDrop = (
    event: React.DragEvent<HTMLDivElement>,
  ) => {
    event.preventDefault();

    setIsDragging(false);

    if (isFull) {
      return;
    }

    const files = Array.from(event.dataTransfer.files);

    if (files.length > 0) {
      onFilesSelected(files);
    }
  };

  return (
    <div
      onDragOver={(event) => {
        event.preventDefault();

        if (!isFull) {
          setIsDragging(true);
        }
      }}
      onDragLeave={() => setIsDragging(false)}
      onDrop={handleDrop}
      className={`
        rounded-2xl
        border-2
        border-dashed
        px-6
        py-10
        text-center
        transition-all
        duration-200

        ${
          isDragging
            ? "border-[#2563EB] bg-blue-50"
            : "border-blue-200 bg-[#FAFCFF]"
        }

        ${
          isFull
            ? "opacity-70"
            : "hover:border-[#2563EB]"
        }
      `}
    >
      <input
        ref={inputRef}
        type="file"
        multiple
        accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        className="hidden"
        onChange={handleFileInput}
      />

      <div
        className="
          mx-auto
          mb-4
          flex
          h-14
          w-14
          items-center
          justify-center
          rounded-full
          bg-blue-50
          text-[#2563EB]
        "
      >
        <FileUp size={28} />
      </div>

      <h3 className="text-base font-semibold text-[#1F2937]">
        {isFull
          ? "10 dosya başarıyla seçildi"
          : "Word dosyalarını buraya sürükleyin"}
      </h3>

      {!isFull && (
        <>
          <p className="mt-1 text-sm text-[#6B7280]">
            veya
          </p>

          <button
            type="button"
            onClick={openFileDialog}
            className="
              mx-auto
              mt-4
              flex
              items-center
              gap-2
              rounded-lg
              bg-[#2563EB]
              px-5
              py-2.5
              text-sm
              font-medium
              text-white
              shadow-sm
              transition
              hover:bg-blue-700
            "
          >
            <Plus size={17} />

            Dosya Seç
          </button>
        </>
      )}

      <p className="mt-5 text-xs text-[#6B7280]">
        Yalnızca .docx uzantılı dosyalar • Tam 10 dosya
      </p>

      <div className="mt-3 text-sm font-semibold">
        <span
          className={
            fileCount === 10
              ? "text-[#16A34A]"
              : "text-[#2563EB]"
          }
        >
          {fileCount}
        </span>

        <span className="text-[#6B7280]">
          {" "}
          / 10 dosya
        </span>
      </div>
    </div>
  );
}

export default FileDropzone;