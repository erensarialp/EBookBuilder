import {
  Check,
  Circle,
  LoaderCircle,
} from "lucide-react";

interface GenerationProgressProps {
  activeStep: number;
}

const steps = [
  {
    title: "Dosyalar yüklendi",
    description: "10 Word belgesi başarıyla alındı.",
  },
  {
    title: "Word belgeleri okunuyor",
    description: "Metin ve paragraf yapısı çıkarılıyor.",
  },
  {
    title: "İletişim bilgileri temizleniyor",
    description:
      "E-posta adresleri ve telefon numaraları kaldırılıyor.",
  },
  {
    title: "İçindekiler hazırlanıyor",
    description:
      "Başlıklar ve başlangıç sayfaları oluşturuluyor.",
  },
  {
    title: "PDF oluşturuluyor",
    description:
      "Dosyalar tek bir e-kitapta birleştiriliyor.",
  },
];

function GenerationProgress({
  activeStep,
}: GenerationProgressProps) {
  return (
    <div
      className="
        mx-auto
        w-full
        max-w-2xl
        rounded-3xl
        border
        border-[#E5E7EB]
        bg-white
        p-6
        shadow-sm
        sm:p-10
      "
    >
      <div className="text-center">
        <LoaderCircle
          size={50}
          className="
            mx-auto
            animate-spin
            text-[#2563EB]
          "
        />

        <h2
          className="
            mt-6
            text-2xl
            font-bold
            text-[#1F2937]
          "
        >
          E-kitabınız hazırlanıyor...
        </h2>

        <p className="mt-2 text-sm text-[#6B7280]">
          Bu işlem birkaç dakika sürebilir.
          Lütfen sayfayı kapatmayın.
        </p>
      </div>

      <div className="mt-10 space-y-2">
        {steps.map((step, index) => {
          const completed = index < activeStep;
          const active = index === activeStep;

          return (
            <div
              key={step.title}
              className="
                flex
                gap-4
                rounded-xl
                p-3
              "
            >
              <div className="pt-0.5">
                {completed ? (
                  <div
                    className="
                      flex
                      h-7
                      w-7
                      items-center
                      justify-center
                      rounded-full
                      bg-[#16A34A]
                      text-white
                    "
                  >
                    <Check size={16} />
                  </div>
                ) : active ? (
                  <div
                    className="
                      flex
                      h-7
                      w-7
                      items-center
                      justify-center
                      rounded-full
                      bg-[#2563EB]
                      text-white
                    "
                  >
                    <LoaderCircle
                      size={15}
                      className="animate-spin"
                    />
                  </div>
                ) : (
                  <Circle
                    size={28}
                    className="text-[#CBD5E1]"
                  />
                )}
              </div>

              <div>
                <p
                  className={`
                    text-sm
                    font-semibold

                    ${
                      active || completed
                        ? "text-[#1F2937]"
                        : "text-[#9CA3AF]"
                    }
                  `}
                >
                  {step.title}
                </p>

                <p
                  className="
                    mt-1
                    text-xs
                    leading-relaxed
                    text-[#6B7280]
                  "
                >
                  {step.description}
                </p>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default GenerationProgress;