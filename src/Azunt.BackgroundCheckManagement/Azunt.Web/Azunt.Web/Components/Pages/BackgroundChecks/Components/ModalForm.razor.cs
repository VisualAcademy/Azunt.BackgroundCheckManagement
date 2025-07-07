using Azunt.BackgroundCheckManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Azunt.Web.Components.Pages.BackgroundChecks.Components;

public partial class ModalForm : ComponentBase
{
    private IBrowserFile? selectedFile;

    public bool IsShow { get; set; } = false;

    public void Show() => IsShow = true;
    public void Hide()
    {
        IsShow = false;
        StateHasChanged();
    }

    [Parameter] public string UserName { get; set; } = "";
    [Parameter] public RenderFragment EditorFormTitle { get; set; } = null!;
    [Parameter] public BackgroundCheck ModelSender { get; set; } = null!;
    public BackgroundCheck ModelEdit { get; set; } = new();

    [Parameter] public Action CreateCallback { get; set; } = null!;
    [Parameter] public EventCallback<bool> EditCallback { get; set; }

    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = "";

    [Inject] public IBackgroundCheckRepository RepositoryReference { get; set; } = null!;
    [Inject] private IBackgroundCheckStorageService StorageService { get; set; } = null!;

    protected override void OnParametersSet()
    {
        ModelEdit = ModelSender != null
            ? new BackgroundCheck
            {
                Id = ModelSender.Id,
                BackgroundCheckId = ModelSender.BackgroundCheckId,
                Provider = ModelSender.Provider,
                Score = ModelSender.Score,
                Status = ModelSender.Status,
                FileName = ModelSender.FileName,
                Active = ModelSender.Active,
                CreatedBy = ModelSender.CreatedBy,
                CreatedAt = ModelSender.CreatedAt
            }
            : new BackgroundCheck();
    }

    protected async Task HandleFileChange(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;

        using var stream = selectedFile.OpenReadStream(10 * 1024 * 1024);
        var savedPath = await StorageService.UploadAsync(stream, selectedFile.Name);
        ModelEdit.FileName = Path.GetFileName(savedPath);
    }

    protected async Task HandleValidSubmit()
    {
        ModelSender.BackgroundCheckId = ModelEdit.BackgroundCheckId;
        ModelSender.Provider = ModelEdit.Provider;
        ModelSender.Score = ModelEdit.Score;
        ModelSender.Status = ModelEdit.Status;
        ModelSender.FileName = ModelEdit.FileName;
        ModelSender.CreatedBy = string.IsNullOrWhiteSpace(UserName) ? "Anonymous" : UserName;
        ModelSender.Active = true;

        if (ModelSender.Id == 0)
        {
            ModelSender.CreatedAt = DateTimeOffset.UtcNow;
            await RepositoryReference.AddAsync(ModelSender);
            CreateCallback?.Invoke();
        }
        else
        {
            await RepositoryReference.UpdateAsync(ModelSender);
            await EditCallback.InvokeAsync(true);
        }

        Hide();
    }
}
