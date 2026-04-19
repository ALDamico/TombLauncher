using System.Globalization;
using TombLauncher.Core.Dtos;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.Ai.Services;

public class AiModelRegistry
{
    public AiModelRegistry()
    {
        _modelLookup = new Dictionary<string, AiModelMetadata>();
        foreach (var model in AvailableModels)
        {
            _modelLookup[model.ModelId] = model;
        }
    }

    private readonly Dictionary<string, AiModelMetadata> _modelLookup;

    public AiModelMetadata? GetMetadata(string? modelId)
    {
        if (modelId == null)
            return null;
        return _modelLookup.GetValueOrDefault(modelId);
    }

    public IReadOnlyList<AiModelMetadata> AvailableModels { get; } =
    [
        // Phi 4 mini
        new AiModelMetadata()
        {
            ModelId = "unsloth/Phi-4-mini-instruct-GGUF",
            Vendor = "Microsoft",
            Description = "PHI_4_MINI_DESCRIPTION".GetLocalizedString(),
            FriendlyName = "Phi 4 Mini",
            AiModelClass = AiModelClass.Lightweight,
            FileName = "Phi-4-mini-instruct-Q4_K_M.gguf",
            DownloadLink =
                "https://huggingface.co/unsloth/Phi-4-mini-instruct-GGUF/resolve/main/Phi-4-mini-instruct-Q4_K_M.gguf",
            SupportedLanguages =
            [
                CultureInfo.GetCultureInfo("ar"), CultureInfo.GetCultureInfo("zh"), CultureInfo.GetCultureInfo("cs"),
                CultureInfo.GetCultureInfo("da"), CultureInfo.GetCultureInfo("nl"), CultureInfo.GetCultureInfo("en"),
                CultureInfo.GetCultureInfo("fi"), CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("de"),
                CultureInfo.GetCultureInfo("he"), CultureInfo.GetCultureInfo("hu"), CultureInfo.GetCultureInfo("it"),
                CultureInfo.GetCultureInfo("ja"), CultureInfo.GetCultureInfo("ko"), CultureInfo.GetCultureInfo("no"),
                CultureInfo.GetCultureInfo("pl"), CultureInfo.GetCultureInfo("pt"), CultureInfo.GetCultureInfo("ru"),
                CultureInfo.GetCultureInfo("es"), CultureInfo.GetCultureInfo("sv"), CultureInfo.GetCultureInfo("th"),
                CultureInfo.GetCultureInfo("tr"), CultureInfo.GetCultureInfo("uk")
            ]
        },

        //Ministral 3B
        new()
        {
            ModelId = "unsloth/Ministral-3-3B-Instruct-2512-GGUF",
            FriendlyName = "Ministral 3 3B",
            AiModelClass = AiModelClass.Lightweight,
            Vendor = "Mistral AI",
            DownloadLink =
                "https://huggingface.co/unsloth/Ministral-3-3B-Instruct-2512-GGUF/resolve/main/Ministral-3-3B-Instruct-2512-Q4_K_M.gguf",
            FileName = "Ministral-3-3B-Instruct-2512-Q4_K_M.gguf",
            Description = "MINISTRAL_3_3B_DESCRIPTION".GetLocalizedString(),
            SupportedLanguages =
            [
                CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("es"),
                CultureInfo.GetCultureInfo("de"), CultureInfo.GetCultureInfo("it"), CultureInfo.GetCultureInfo("pt"),
                CultureInfo.GetCultureInfo("nl"), CultureInfo.GetCultureInfo("zh"), CultureInfo.GetCultureInfo("ja"),
                CultureInfo.GetCultureInfo("ko"), CultureInfo.GetCultureInfo("ar")
            ]
        },
        new()
        {
            ModelId = "bartowski/Meta-Llama-3.1-8B-Instruct-GGUF",
            DownloadLink =
                "https://huggingface.co/bartowski/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf",
            FileName = "Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf",
            Vendor = "Meta",
            Description = "LLAMA_3_1_8B_DESCRIPTION".GetLocalizedString(),
            FriendlyName = "Llama 3.1 8B",
            AiModelClass = AiModelClass.Mainstream,
            SupportedLanguages =
            [
                CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("de"), CultureInfo.GetCultureInfo("fr"),
                CultureInfo.GetCultureInfo("it"), CultureInfo.GetCultureInfo("pt"), CultureInfo.GetCultureInfo("hi"),
                CultureInfo.GetCultureInfo("es"), CultureInfo.GetCultureInfo("th")
            ]
        },
        new()
        {
            ModelId = "microsoft/phi-4-gguf",
            Description = "PHI_4_14B_DESCRIPTION".GetLocalizedString(),
            FriendlyName = "Phi 4",
            Vendor = "Microsoft",
            AiModelClass = AiModelClass.Mainstream,
            SupportedLanguages = [CultureInfo.GetCultureInfo("en")],
            DownloadLink = "https://huggingface.co/microsoft/phi-4-gguf/resolve/main/phi-4-Q4_K.gguf",
            FileName = "phi-4-Q4_K.gguf"
        },
        new()
        {
            ModelId = "unsloth/Qwen3.5-35B-A3B-GGUF",
            Description = "QWEN_3_5_35B_DESCRIPTION".GetLocalizedString(),
            FriendlyName = "Qwen 3.5 35B",
            Vendor = "Alibaba Cloud",
            AiModelClass = AiModelClass.Performance,
            SupportedLanguages =
            [
                CultureInfo.GetCultureInfo("af"), CultureInfo.GetCultureInfo("ar"), CultureInfo.GetCultureInfo("as"),
                CultureInfo.GetCultureInfo("az"), CultureInfo.GetCultureInfo("ba"), CultureInfo.GetCultureInfo("be"),
                CultureInfo.GetCultureInfo("bg"), CultureInfo.GetCultureInfo("bn"), CultureInfo.GetCultureInfo("bs"),
                CultureInfo.GetCultureInfo("ca"), CultureInfo.GetCultureInfo("cs"), CultureInfo.GetCultureInfo("cy"),
                CultureInfo.GetCultureInfo("da"), CultureInfo.GetCultureInfo("de"), CultureInfo.GetCultureInfo("el"),
                CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("es"), CultureInfo.GetCultureInfo("et"),
                CultureInfo.GetCultureInfo("eu"), CultureInfo.GetCultureInfo("fa"), CultureInfo.GetCultureInfo("fi"),
                CultureInfo.GetCultureInfo("fo"), CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("ga"),
                CultureInfo.GetCultureInfo("gl"), CultureInfo.GetCultureInfo("gu"), CultureInfo.GetCultureInfo("he"),
                CultureInfo.GetCultureInfo("hi"), CultureInfo.GetCultureInfo("hr"), CultureInfo.GetCultureInfo("ht"),
                CultureInfo.GetCultureInfo("hu"), CultureInfo.GetCultureInfo("hy"), CultureInfo.GetCultureInfo("id"),
                CultureInfo.GetCultureInfo("is"), CultureInfo.GetCultureInfo("it"), CultureInfo.GetCultureInfo("ja"),
                CultureInfo.GetCultureInfo("ka"), CultureInfo.GetCultureInfo("kk"), CultureInfo.GetCultureInfo("km"),
                CultureInfo.GetCultureInfo("kn"), CultureInfo.GetCultureInfo("ko"), CultureInfo.GetCultureInfo("lb"),
                CultureInfo.GetCultureInfo("lo"), CultureInfo.GetCultureInfo("lt"), CultureInfo.GetCultureInfo("lv"),
                CultureInfo.GetCultureInfo("mk"), CultureInfo.GetCultureInfo("ml"), CultureInfo.GetCultureInfo("mr"),
                CultureInfo.GetCultureInfo("ms"), CultureInfo.GetCultureInfo("mt"), CultureInfo.GetCultureInfo("my"),
                CultureInfo.GetCultureInfo("nb"), CultureInfo.GetCultureInfo("ne"), CultureInfo.GetCultureInfo("nl"),
                CultureInfo.GetCultureInfo("nn"), CultureInfo.GetCultureInfo("or"), CultureInfo.GetCultureInfo("pa"),
                CultureInfo.GetCultureInfo("pl"), CultureInfo.GetCultureInfo("pt"), CultureInfo.GetCultureInfo("ro"),
                CultureInfo.GetCultureInfo("ru"), CultureInfo.GetCultureInfo("sd"), CultureInfo.GetCultureInfo("si"),
                CultureInfo.GetCultureInfo("sk"), CultureInfo.GetCultureInfo("sl"), CultureInfo.GetCultureInfo("sq"),
                CultureInfo.GetCultureInfo("sr"), CultureInfo.GetCultureInfo("sv"), CultureInfo.GetCultureInfo("sw"),
                CultureInfo.GetCultureInfo("ta"), CultureInfo.GetCultureInfo("te"), CultureInfo.GetCultureInfo("tg"),
                CultureInfo.GetCultureInfo("th"), CultureInfo.GetCultureInfo("tl"), CultureInfo.GetCultureInfo("tr"),
                CultureInfo.GetCultureInfo("tt"), CultureInfo.GetCultureInfo("uk"), CultureInfo.GetCultureInfo("ur"),
                CultureInfo.GetCultureInfo("uz"), CultureInfo.GetCultureInfo("vi"), CultureInfo.GetCultureInfo("yi"),
                CultureInfo.GetCultureInfo("zh"),
            ],
            DownloadLink =
                "https://huggingface.co/unsloth/Qwen3.5-35B-A3B-GGUF/resolve/main/Qwen3.5-35B-A3B-UD-IQ4_NL.gguf",
            FileName = "Qwen3.5-35B-A3B-UD-IQ4_NL.gguf"
        },
        new()
        {
            ModelId = "bartowski/c4ai-command-r-08-2024-GGUF",
            FriendlyName = "Command R 32B",
            Description = "COMMAND_R_32B_DESCRIPTION".GetLocalizedString(),
            Vendor = "Cohere",
            AiModelClass = AiModelClass.Performance,
            SupportedLanguages =
                [
                CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("es"),
            CultureInfo.GetCultureInfo("it"), CultureInfo.GetCultureInfo("de"), CultureInfo.GetCultureInfo("pt"),
            CultureInfo.GetCultureInfo("ja"), CultureInfo.GetCultureInfo("ko"), CultureInfo.GetCultureInfo("ar"),
            CultureInfo.GetCultureInfo("zh"),
            ],
            DownloadLink = "https://huggingface.co/bartowski/c4ai-command-r-08-2024-GGUF/resolve/main/c4ai-command-r-08-2024-Q4_K_M.gguf",
            FileName = "c4ai-command-r-08-2024-Q4_K_M.gguf"
        }
    ];
}