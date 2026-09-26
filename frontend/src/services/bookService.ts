import type { BookResponse } from "../types/book";
import type { UploadedDocument } from "../types/document";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL;

export async function createBook(
  bookName: string,
  documents: UploadedDocument[],
): Promise<BookResponse> {
  const formData = new FormData();

  formData.append(
    "BookName",
    bookName,
  );

  documents.forEach((document) => {
    formData.append(
      "Files",
      document.file,
      document.file.name,
    );
  });

  const response = await fetch(
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

      if (errorBody?.message) {
        message =
          errorBody.message;
      }
    } catch {
      // Response JSON değilse
      // varsayılan mesaj kullanılır.
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