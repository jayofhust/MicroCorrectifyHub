using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecALLDemo.Contrib.TextItem.Api.Commands;
using RecALLDemo.Contrib.TextItem.Api.Services;
using RecALLDemo.Contrib.TextItem.Api.ViewModels;
using TheSalLab.GeneralReturnValues;

namespace RecALLDemo.Contrib.TextItem.Api.Controller;


//web服务的controller 只返回json

[ApiController] 
[Authorize]
[Route("[controller]")]
public class ItemController{
    
    private readonly IIdentityService _identityService;
    private readonly TextItemContext _textItemContext;
    
    private readonly ILogger<ItemController> _logger;


    public ItemController(IIdentityService identityService,
        TextItemContext textItemContext, ILogger<ItemController> logger) {
        _identityService = identityService;
        _textItemContext = textItemContext;
        _logger = logger;
    }

    
    
    //[FromBody]将post的json转换成对象
    [Route("create")]
    [HttpPost]
    public async Task<ActionResult<ServiceResultViewModel<string>>> CreateAsync(
        [FromBody] CreateTextItemCommand command) {
        
        _logger.LogInformation(
            "----- Handling command {CommandName} ({@Command})",
            command.GetType().Name, command);

        var textItem = new Models.TextItem {
            Content = command.Content,
            UserIdentityGuid = _identityService.GetUserIdentityGuid(),
            IsDeleted = false
        };
        var textItemEntity = _textItemContext.Add(textItem);  //加入对象
        await _textItemContext.SaveChangesAsync();            //调用async函数
        
        
        _logger.LogInformation("----- Command {CommandName} handled",
            command.GetType().Name); //handle时候log一次 结束时候log一次

        return ServiceResult<string>.CreateSucceededResult(textItemEntity.Entity.Id.ToString())
            .ToServiceResultViewModel();

    }
    
    
    /*[Route("get/{id}")]
    [HttpGet]
    public async Task<ActionResult<Models.TextItem>> GetAsync(int id) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.Id == id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);
        
        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem {id}");
        }
        
        return textItem is null ? new BadRequestResult() : textItem;
    }

    [Route("getByItemId/{itemId}")]
    [HttpGet]
    public async Task<ActionResult<Models.TextItem>> GetByItemId(int itemId) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();
        
        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.ItemId == itemId && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);
        
        
        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem, ItemID: {itemId}");
        }


        return textItem is null ? new BadRequestResult() : textItem;
    }
    
    
    [Route("getItems")]
    [HttpPost]
    public async Task<ActionResult<IEnumerable<Models.TextItem>>> GetItemsAsync(
        GetItemsCommand command) {
        var itemIds = command.Ids.ToList();
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItems = await _textItemContext.TextItems.Where(p =>
                p.ItemId.HasValue && itemIds.Contains(p.ItemId.Value) &&
                p.UserIdentityGuid == userIdentityGuid && !p.IsDeleted)
            .ToListAsync();
        //select * from textitems
        //where itemid in (1,4,5) and UserIdentityGuid = '' and IsDeleted = 0
        
        if (textItems.Count != itemIds.Count) {
            var missingIds = string.Join(",",
                itemIds.Except(textItems.Select(p => p.ItemId.Value))
                    .Select(p => p.ToString()));
            
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem {missingIds}");


            return new BadRequestResult();
            //当发现可能有问题的操作时，直接refuse
        }

        textItems.Sort((x, y) =>
            itemIds.IndexOf(x.ItemId.Value) - itemIds.IndexOf(y.ItemId.Value));
        return textItems;
    }*/
    
    [Route("get/{id}")]
    [HttpGet]
    // ServiceResultViewModel<TextItemViewModel>
    public async Task<ActionResult<ServiceResultViewModel<TextItemViewModel>>>
        GetAsync(int id) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.Id == id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem {id}");

            return ServiceResult<TextItemViewModel>
                .CreateFailedResult($"Unknown TextItem id: {id}")
                .ToServiceResultViewModel();
        }

        return ServiceResult<TextItemViewModel>
                .CreateSucceededResult(new TextItemViewModel {
                    Id = textItem.Id,
                    ItemId = textItem.ItemId,
                    Content = textItem.Content
                }).ToServiceResultViewModel();
    }

    [Route("getByItemId/{itemId}")]
    [HttpGet]
    // ServiceResultViewModel<TextItemViewModel>
    public async Task<ActionResult<ServiceResultViewModel<TextItemViewModel>>>
        GetByItemId(int itemId) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.ItemId == itemId && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem, ItemID: {itemId}");

            return ServiceResult<TextItemViewModel>
                .CreateFailedResult($"Unknown TextItem with ItemID: {itemId}")
                .ToServiceResultViewModel();
        }

        return ServiceResult<TextItemViewModel>
                .CreateSucceededResult(new TextItemViewModel {
                    Id = textItem.Id,
                    ItemId = textItem.ItemId,
                    Content = textItem.Content
                }).ToServiceResultViewModel();
    }

    [Route("getItems")]
    [HttpPost]
    public async
        Task<ActionResult<
            ServiceResultViewModel<IEnumerable<TextItemViewModel>>>>
        GetItemsAsync(GetItemsCommand command) {
        var itemIds = command.Ids.ToList();
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItems = await _textItemContext.TextItems.Where(p =>
                p.ItemId.HasValue && itemIds.Contains(p.ItemId.Value) &&
                p.UserIdentityGuid == userIdentityGuid && !p.IsDeleted)
            .ToListAsync();

        if (textItems.Count != itemIds.Count) {
            var missingIds = string.Join(",",
                itemIds.Except(textItems.Select(p => p.ItemId.Value))
                    .Select(p => p.ToString()));

            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem {missingIds}");

            return ServiceResult<IEnumerable<TextItemViewModel>>
                .CreateFailedResult($"Unknown Item id: {missingIds}")
                .ToServiceResultViewModel();
        }

        textItems.Sort((x, y) =>
            itemIds.IndexOf(x.ItemId.Value) - itemIds.IndexOf(y.ItemId.Value));

        return ServiceResult<IEnumerable<TextItemViewModel>>
            .CreateSucceededResult(textItems.Select(p => new TextItemViewModel {
                Id = p.Id, ItemId = p.ItemId, Content = p.Content
            })).ToServiceResultViewModel();
    }

    
    
    // Task<Actionresult>没有返回值，至少返回一个状态码
    [Route("update")]
    [HttpPost]
    public async Task<ServiceResultViewModel> UpdateAsync(
        [FromBody] UpdateTextItemCommand command) {
        _logger.LogInformation(
            "----- Handling command {CommandName} ({@Command})",
            command.GetType().Name, command);

        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.Id == command.Id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的TextItem {command.Id}");

            return ServiceResult
                .CreateFailedResult($"Unknown TextItem id: {command.Id}")
                .ToServiceResultViewModel();
        }

        textItem.Content = command.Content;
        await _textItemContext.SaveChangesAsync();

        _logger.LogInformation("----- Command {CommandName} handled",
            command.GetType().Name);

        return ServiceResult.CreateSucceededResult().ToServiceResultViewModel();
    }


    
    
    
    [Route("delete")]
    [HttpPost]
    public async Task<ServiceResultViewModel> DeleteAsync(
        [FromBody] DeleteTextItemCommand command) 
    {
        
        _logger.LogInformation(
            "----- Handling command {CommandName} ({@Command})",
            command.GetType().Name, command);
        
        
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var textItem = await _textItemContext.TextItems.FirstOrDefaultAsync(p =>
            p.Id == command.id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);
        
        if (textItem is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看的东西 {command.id} 已删除、不允许重复删除");

            return ServiceResult
                .CreateFailedResult($"id:{command.id}已经删除，无法重复删除")
                .ToServiceResultViewModel();
        }
        
        textItem.IsDeleted = true;
        await _textItemContext.SaveChangesAsync();
        
        _logger.LogInformation("----- Command {CommandName} handled",
            command.GetType().Name);
        
        return ServiceResult.CreateSucceededResult().ToServiceResultViewModel();
    }

    
}