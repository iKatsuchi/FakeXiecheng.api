using FakeXiecheng.api.Dtos;
using FakeXiecheng.api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using AutoMapper;
using System.Collections.Generic;
using FakeXiecheng.api.ResourceParameters;
using FakeXiecheng.api.Models;
using Microsoft.AspNetCore.JsonPatch;
using FakeXiecheng.api.Helper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net.Http.Headers;

namespace FakeXiecheng.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutesController : ControllerBase//Controller 可以提供视图 用不到
    {
        private readonly ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;

        public TouristRoutesController(ITouristRouteRepository touristRouteRepository,
            IMapper mapper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPropertyMappingService propertyMappingService)
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _propertyMappingService = propertyMappingService;
        }

        private string GenerateTouristRouteResourceURL(
            TouristRouteResourceParameters parameters,
            PaginationResourceParamaters parameter2,
            ResourceUrlType type)
        {
            return type switch
            {
                ResourceUrlType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                new
                {
                    fields = parameters.Fields,
                    orderBy = parameters.OrderBy,
                    keyword = parameters.Keyword,
                    rating = parameters.Rating,
                    pageNumber = parameter2.PageNumber - 1,
                    pageSize = parameter2.PageSize
                }),
                ResourceUrlType.NextPage => _urlHelper.Link("GetTouristRoutes",
                new
                {
                    fields = parameters.Fields,
                    orderBy = parameters.OrderBy,
                    keyword = parameters.Keyword,
                    rating = parameters.Rating,
                    pageNumber = parameter2.PageNumber + 1,
                    pageSize = parameter2.PageSize
                }),
                _ => _urlHelper.Link("GetTouristRoutes",
                new
                {
                    fields = parameters.Fields,
                    orderBy = parameters.OrderBy,
                    keyword = parameters.Keyword,
                    rating = parameters.Rating,
                    pageNumber = parameter2.PageNumber + 1,
                    pageSize = parameter2.PageSize
                }),
            };
        }
        //api/touristRoutes?keyword=传入的参数
        [HttpGet(Name = "GetTouristRoutesAsync")]
        [HttpHead]
        //写了    [ApiController]就可以省略[FromQuery]此处为了说明问题特别标明
        // application/vnd._{公司名称}.hateoas+json
        // 1、 application/json 旅游路线资源
        // 2、 application/vnd.xxx.hateoas+json 附加超链接
        public async Task<IActionResult> GetTouristRoutesAsync(
            [FromQuery] TouristRouteResourceParameters parameters,
            [FromQuery] PaginationResourceParamaters parameter2,
            [FromHeader(Name ="Accept")] string mediaType
            //[FromQuery] string keyword
            //string rating //小于lessThan, 大于lagerThan,等于equalTo lessThan3,largeThan2
            )
        {
            //Regex regex = new Regex(@"([A-Za-z0-9\-]+)(\d+)");
            //string operatorType="";
            //int ratingValue=-1;
            //Match match=regex.Match(parameters.Rating);
            //if(match.Success)
            //{
            //    operatorType = match.Groups[1].Value;
            //    ratingValue = int.Parse(match.Groups[2].Value);
            //}

            if(!MediaTypeHeaderValue.TryParse(mediaType,out MediaTypeHeaderValue parsedMediatype))
            {
                return BadRequest();
            }

            if(!_propertyMappingService
                .IsMappingExists<TouristRouteDto,TouristRoute>(
                parameters.OrderBy))
            {
                return BadRequest("请输入正确的排序参数");
            }

            //参数塑性验证
            if(!_propertyMappingService.IsPropertiesExists<TouristRouteDto>(parameters.Fields))
            {
                return BadRequest("请输入正确的塑性参数");
            }

            var touristRoutesFromRepo = await _touristRouteRepository
                .GetTouristRoutesAsync(
                parameters.Keyword, 
                parameters.RatingOperator, 
                parameters.RatingValue,
                parameter2.PageSize,
                parameter2.PageNumber,
                parameters.OrderBy);
            if (touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
            {
                return NotFound("没有找到合适的旅游路线");
            }
            var touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            var previousPageLink = touristRoutesFromRepo.HasPrevious
                ? GenerateTouristRouteResourceURL(
                    parameters, parameter2, ResourceUrlType.PreviousPage)
                : null;
            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(
                    parameters, parameter2, ResourceUrlType.NextPage)
                : null;
            // 添加x-pagination头部添加
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.TotalCount,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalCount
            };

            Response.Headers.Add("x-pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var shapeDtoList = touristRoutesDto.ShapeData(parameters.Fields);

            if(parsedMediatype.MediaType == "application/vnd.xxx.haseoas+json")
            {
                var linkDto = CreateLinksForTouristRouteList(parameters, parameter2);

                var shapedDtoWithLinklist = shapeDtoList.Select(t =>
                {
                    var touristRouteDictionary = t as IDictionary<string, object>;
                    var links = CreateLinkForTouristRoute(
                        (Guid)touristRouteDictionary["Id"], null);
                    touristRouteDictionary.Add("links", links);

                    return touristRouteDictionary;
                });

                var result = new
                {
                    value = shapedDtoWithLinklist,
                    links = linkDto
                };

                return Ok(result);
            }

            return Ok(shapeDtoList);
        }

        private IEnumerable<LinkDto> CreateLinksForTouristRouteList(
            TouristRouteResourceParameters parameters,
            PaginationResourceParamaters parameter2)
        {
            var links = new List<LinkDto>();
            //添加self，自我链接
            links.Add(new LinkDto(
                GenerateTouristRouteResourceURL(parameters, parameter2, ResourceUrlType.CurrentPage),
                "self",
                "Get"
                ));
            links.Add(new LinkDto(
                Url.Link("CreateTouristRoute", null),
                "create_tourist_route",
                "POST"
                ));
            return links;
        }

        [HttpGet("{touristRouteId}", Name = "GetTouristRouteById")]
        [HttpHead]
        public async Task<IActionResult> GetTouristRouteByIdAsync(Guid touristRouteId, string fields)
        {
            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            if (touristRouteFromRepo == null)
            {
                return NotFound($"旅游路线{touristRouteFromRepo}找不到");
            }
            //var touristRouteDto = new TouristRouteDto()
            //{
            //    Id = touristRouteFromRepo.Id,
            //    Title = touristRouteFromRepo.Title,
            //    Description = touristRouteFromRepo.Description,
            //    Price= touristRouteFromRepo.OriginalPrice*(decimal)(touristRouteFromRepo.DiscountPresent??1),
            //    CreateTime=touristRouteFromRepo.CreateTime,
            //    UpdateTime=touristRouteFromRepo.UpdateTime,
            //    Features=touristRouteFromRepo.Features,
            //    Fees=touristRouteFromRepo.Fees,
            //    Notes=touristRouteFromRepo.Notes,
            //    Rating=touristRouteFromRepo.Rating,
            //    TravelDays=touristRouteFromRepo.TravelDays.ToString(),
            //    TripType=touristRouteFromRepo.TripType.ToString(),
            //    DepartureCity=touristRouteFromRepo.DepartureCity.ToString(),
            //};
            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);
            //超链接dto
            var linkDtos = CreateLinkForTouristRoute(touristRouteId, fields);
            var result = touristRouteDto.ShapeData(fields) as IDictionary<string, object>;

            result.Add("links", linkDtos);
            return Ok(result);
        }

        private IEnumerable<LinkDto> CreateLinkForTouristRoute(
            Guid touristRouteId,
            string fields)
        {
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(
                    Url.Link("GetTouristRouteById", new {touristRouteId, fields}),
                    "self",
                    "GET"));

            links.Add(
                new LinkDto(
                    Url.Link("UpdateTouristRoute", new { touristRouteId }),
                    "update",
                    "PUT"));

            links.Add(
                new LinkDto(
                    Url.Link("PartiallyUpdateTouristRoute", new { touristRouteId }),
                    "partially_update",
                    "PATCH"));

            links.Add(
                new LinkDto(
                    Url.Link("GetPictureListForTouristRoute", new { touristRouteId }),
                    "get_pictures",
                    "GET"));

            links.Add(
                new LinkDto(
                    Url.Link("CreateTouristRoutePicture", new { touristRouteId }),
                    "create_picture",
                    "POST"));


            return links;
        }

        [HttpPost(Name = "CreateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles ="Admin")]
        //[Authorize]
        public async Task<IActionResult> CreateTouristRouteAsync([FromBody] TouristRouteForCreateionDto touristRouteForCreateionDto)
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreateionDto);
            _touristRouteRepository.AddTouristRoute(touristRouteModel);
            await _touristRouteRepository.SaveAsync();
            var touristRouteToReturn = _mapper.Map<TouristRouteDto>(touristRouteModel);

            var links = CreateLinkForTouristRoute(touristRouteModel.Id, null);

            var result = touristRouteToReturn.ShapeData(null)
                as IDictionary<string, object>;

            result.Add("links", links);

            return CreatedAtRoute("GetTouristRouteById",
                new { touristRouteId = result["Id"] },
                result);
        }

        [HttpPut("{touristRouteId}", Name = "UpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTouristRouteAsync(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRouteForUpdateDto touristRouteForUpdateDto
            )
        {
            if (! await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线并不存在");
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            //1、映射dto
            //2、更新Dto
            //3、映射Model
            _mapper.Map(touristRouteForUpdateDto, touristRouteFromRepo);

            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{touristRouteId}",Name = "PartiallyUpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PartiallyUpdateTouristRouteAsync(
            [FromRoute] Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument)
        {
            if (! await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线并不存在");
            }
            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo);
            patchDocument.ApplyTo(touristRouteToPatch, ModelState);
            if (!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(touristRouteToPatch, touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}",Name = "DeleteTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristRouteAsync([FromRoute] Guid touristRouteId)
        {
            if (! await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅游路线并不存在");
            }
            var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            _touristRouteRepository.DeleteTouristRoute(touristRoute);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("({touristIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteByIDsAsync(
        [ModelBinder(BinderType =typeof(ArrayModelBinder))]
        [FromRoute]IEnumerable<Guid> touristIDs)
        {
            if(touristIDs==null)
            {
                return BadRequest();
            }

            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _touristRouteRepository.DeleteTouristRoutes(touristRoutesFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
