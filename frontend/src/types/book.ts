export interface PaperResponse {
  id: number;
  fileName: string;
  title: string;
  orderIndex: number;
  startPage: number | null;
}

export interface BookResponse {
  id: number;
  name: string;
  status: string;
  pdfPath: string | null;
  papers: PaperResponse[];
}