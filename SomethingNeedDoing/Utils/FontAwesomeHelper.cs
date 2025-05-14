using Dalamud.Interface;

namespace SomethingNeedDoing.Utils;

/// <summary>
/// Helper class for standardizing Font Awesome icon usage across the application.
/// </summary>
public static class FontAwesomeHelper
{
    // Macro types
    public const FontAwesomeIcon IconNativeMacro = FontAwesomeIcon.FileCode;
    public const FontAwesomeIcon IconLuaMacro = FontAwesomeIcon.Moon;
    public const FontAwesomeIcon IconGitMacro = FontAwesomeIcon.CodeBranch;

    // Action icons
    public const FontAwesomeIcon IconPlay = FontAwesomeIcon.Play;
    public const FontAwesomeIcon IconStop = FontAwesomeIcon.Stop;
    public const FontAwesomeIcon IconPause = FontAwesomeIcon.Pause;
    public const FontAwesomeIcon IconResume = FontAwesomeIcon.Play; // Same as IconPlay
    public const FontAwesomeIcon IconEdit = FontAwesomeIcon.Edit;
    public const FontAwesomeIcon IconCopy = FontAwesomeIcon.Copy;
    public const FontAwesomeIcon IconDelete = FontAwesomeIcon.TrashAlt;
    public const FontAwesomeIcon IconRename = FontAwesomeIcon.Pen;
    public const FontAwesomeIcon IconDuplicate = FontAwesomeIcon.Clone;
    public const FontAwesomeIcon IconNew = FontAwesomeIcon.Plus;
    public const FontAwesomeIcon IconImport = FontAwesomeIcon.FileImport;

    // Navigation icons
    public const FontAwesomeIcon IconFolder = FontAwesomeIcon.Folder;
    public const FontAwesomeIcon IconFolderOpen = FontAwesomeIcon.FolderOpen;
    public const FontAwesomeIcon IconCollapsed = FontAwesomeIcon.ChevronRight;
    public const FontAwesomeIcon IconExpanded = FontAwesomeIcon.ChevronDown;
    public const FontAwesomeIcon IconHome = FontAwesomeIcon.Home;

    // Tab icons
    public const FontAwesomeIcon IconMacros = FontAwesomeIcon.FileAlt;
    public const FontAwesomeIcon IconRunning = FontAwesomeIcon.PlayCircle;
    public const FontAwesomeIcon IconHelp = FontAwesomeIcon.QuestionCircle;
    public const FontAwesomeIcon IconSettings = FontAwesomeIcon.Cog;

    // Status icons
    public const FontAwesomeIcon IconRunningStatus = FontAwesomeIcon.Spinner;
    public const FontAwesomeIcon IconPausedStatus = FontAwesomeIcon.PauseCircle;
    public const FontAwesomeIcon IconCompletedStatus = FontAwesomeIcon.CheckCircle;
    public const FontAwesomeIcon IconErrorStatus = FontAwesomeIcon.ExclamationTriangle;

    // Search and filter
    public const FontAwesomeIcon IconSearch = FontAwesomeIcon.Search;
    public const FontAwesomeIcon IconFilter = FontAwesomeIcon.Filter;
    public const FontAwesomeIcon IconClear = FontAwesomeIcon.TimesCircle;
    public const FontAwesomeIcon IconSortAsc = FontAwesomeIcon.SortAmountUp;
    public const FontAwesomeIcon IconSortDesc = FontAwesomeIcon.SortAmountDown;

    // Settings controls
    public const FontAwesomeIcon IconToggleOn = FontAwesomeIcon.ToggleOn;
    public const FontAwesomeIcon IconToggleOff = FontAwesomeIcon.ToggleOff;

    // Collapsible panels
    public const FontAwesomeIcon IconCollapsedPanel = FontAwesomeIcon.PlusSquare;
    public const FontAwesomeIcon IconExpandedPanel = FontAwesomeIcon.MinusSquare;
}
