using Azunt.Components.Dialogs;
using Azunt.Components.Paging;
using Azunt.BackgroundCheckManagement;
using Azunt.Web.Components.Pages.BackgroundChecks.Components;
using Azunt.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azunt.Web.Pages.BackgroundChecks;

public partial class BackgroundCheckManager : ComponentBase
{
    public ModalForm EditorFormReference { get; set; } = null!;
    public DeleteDialog DeleteDialogReference { get; set; } = null!;
    public List<BackgroundCheck> models = new();
    public BackgroundCheck model = new();
    public string EditorFormTitle { get; set; } = "CREATE";
    private string sortOrder = "";
    private string searchQuery = "";
    private int timeZoneOffsetMinutes;

    protected PagerBase pager = new()
    {
        PageIndex = 0,
        PageNumber = 1,
        PageSize = 10,
        PagerButtonCount = 5
    };

    [Inject] public IBackgroundCheckRepository RepositoryReference { get; set; } = null!;
    [Inject] public IBackgroundCheckStorageService BackgroundCheckStorage { get; set; } = null!;
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; } = null!;
    [Inject] public IConfiguration Configuration { get; set; } = null!;
    [Inject] public BackgroundCheckDbContextFactory DbContextFactory { get; set; } = null!;
    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = null!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = null!;

    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = "";
    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string UserName { get; set; } = "";
    [Parameter] public string Category { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserName))
            await GetUserIdAndUserName();

        await DisplayData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            timeZoneOffsetMinutes = await JSRuntimeInjector.InvokeAsync<int>("Azunt.TimeZone.getLocalOffsetMinutes");
            StateHasChanged();
        }
    }

    private async Task DisplayData()
    {
        var result = await RepositoryReference.GetAllAsync<int>(
            pager.PageIndex, pager.PageSize,
            "", searchQuery, sortOrder,
            ParentId, Category);

        pager.RecordCount = result.TotalCount;
        models = result.Items.ToList();

        StateHasChanged();
    }

    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new BackgroundCheck();
        EditorFormReference.Show();
    }

    protected void EditBy(BackgroundCheck m)
    {
        EditorFormTitle = "EDIT";
        model = m;
        EditorFormReference.Show();
    }

    protected void DeleteBy(BackgroundCheck m)
    {
        model = m;
        DeleteDialogReference.Show();
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();
        await Task.Delay(50);
        model = new BackgroundCheck();
        await DisplayData();
    }

    protected async void DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
            await BackgroundCheckStorage.DeleteAsync(model.FileName);

        await RepositoryReference.DeleteAsync(model.Id);
        DeleteDialogReference.Hide();
        model = new BackgroundCheck();
        await DisplayData();
    }

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query;
        await DisplayData();
    }

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;
        await DisplayData();
    }

    protected async void SortBy(string column)
    {
        if (!sortOrder.StartsWith(column)) sortOrder = "";

        sortOrder = sortOrder switch
        {
            "" => column,
            var val when val == column => $"{column}Desc",
            _ => ""
        };

        await DisplayData();
    }

    private async Task MoveUp(long id)
    {
        await RepositoryReference.MoveUpAsync(id);
        await DisplayData();
    }

    private async Task MoveDown(long id)
    {
        await RepositoryReference.MoveDownAsync(id);
        await DisplayData();
    }

    private void ExportExcel()
    {
        Nav.NavigateTo("/api/BackgroundCheckExport/Excel", forceLoad: true);
    }

    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            UserId = currentUser?.Id ?? "";
            UserName = user.Identity?.Name ?? "Anonymous";
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
}
