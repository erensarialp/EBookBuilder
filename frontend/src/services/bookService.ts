import type {
  BookResponse,
} from "../types/book";

import type {
  UploadedDocument,
} from "../types/document";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL;

if (!API_BASE_URL) {
  throw new Error(
    "VITE_API_BASE_URL tanımlı değil. frontend/.env dosyasını kontrol edin.",
  );
}

export async function createBook(
  bookName: string,
  documents: UploadedDocument[],
): Promise<BookResponse> {
  const formData =
    new FormData();

  formData.append(
    "BookName",
    bookName,
  );

  documents.forEach(
    (document) => {
      formData.append(
        "Files",
        document.file,
        document.file.name,
      );
    },
  );

  const response =
    await fetch(
      `${API_BASE_URL}/api/Books`,
      {
        method: "POST",
        body: formData,
      },
    );

  if (!response.ok) {
    let message =
      "E-kitap oluşturulurken bir hata oluştu.";

    try {
      const errorBody =
        await response.json();

      if (
        typeof errorBody?.message ===
        "string"
      ) {
        message =
          errorBody.message;
      } else if (
        typeof errorBody?.title ===
        "string"
      ) {
        message =
          errorBody.title;
      }
    } catch {
      message =
        "E-kitap oluşturulurken bir hata oluştu.";
    }

    throw new Error(message);
  }

  return response.json();
}

export function getPdfUrl(
  pdfPath: string,
): string {
  return `${API_BASE_URL}${pdfPath}`;
}