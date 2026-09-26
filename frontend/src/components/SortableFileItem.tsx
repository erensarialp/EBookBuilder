import {
  FileText,
  GripVertical,
  Trash2,
} from "lucide-react";

import { useSortable } from "@dnd-kit/react/sortable";

import type { UploadedDocument } from "../types/document";

interface SortableFileItemProps {
  item: UploadedDocument;
  index: number;
  onRemove: (id: string) => void;
}

function SortableFileItem({
  item,
  index,
  onRemove,
}: SortableFileItemProps) {
  const sortable = useSortable({
    id: item.id,
    index,
  });

  const formatFileSize = (bytes: number) => {
    const kb = bytes / 1024;

    if (kb < 1024) {
      return `${kb.toFixed(1)} KB`;
    }

    return `${(kb / 1024).toFixed(1)} MB`;
  };

  return (
    <div
      ref={sortable.ref}
      className={`
        group
        flex
        items-center
        gap-3
        rounded-xl
        border
        bg-white
        px-3
        py-3
        transition-all
        duration-200

        ${
          sortable.isDragging
            ? "scale-[1.01] border-blue-400 shadow-lg opacity-80"
            : "border-[#E5E7EB] hover:border-blue-200 hover:shadow-sm"
        }
      `}
    >
      <button
        ref={sortable.handleRef}
        type="button"
        title="Sıralamak için sürükleyin"
        className="
          flex
          h-9
          w-8
          shrink-0
          items-center
          justify-center
          rounded-md
          text-[#9CA3AF]
          transition
          hover:bg-gray-100
          hover:text-[#374151]
          active:cursor-grabbing
        "
      >
        <GripVertical size={18} />
      </button>

      <div
        className="
          flex
          h-8
          w-8
          shrink-0
          items-center
          justify-center
          rounded-full
          bg-[#F3F4F6]
          text-xs
          font-semibold
          text-[#4B5563]
        "
      >
        {index + 1}
      </div>

      <div
        className="
          hidden
          h-9
          w-9
          shrink-0
          items-center
          justify-center
          rounded-lg
          bg-blue-50
          text-[#2563EB]
          sm:flex
        "
      >
        <FileText size={18} />
      </div>

      <div className="min-w-0 flex-1">
        <p
          className="
            truncate
            text-sm
            font-medium
            text-[#1F2937]
          "
        >
          {item.name}
        </p>

        <p className="mt-0.5 text-xs text-[#9CA3AF]">
          {formatFileSize(item.size)}
        </p>
      </div>

      <button
        type="button"
        onClick={() => onRemove(item.id)}
        title="Dosyayı kaldır"
        className="
          flex
          h-9
          w-9
          shrink-0
          items-center
          justify-center
          rounded-lg
          text-[#DC2626]
          transition
          hover:bg-red-50
        "
      >
        <Trash2 size={17} />
      </button>
    </div>
  );
}

export default SortableFileItem;