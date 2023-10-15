using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using NLayerApp.Core.DTOs;
using NLayerApp.Core.Models;
using NLayerApp.Core.Repositories;
using NLayerApp.Core.Services;
using NLayerApp.Core.UnitOfWorks;
using NLayerApp.Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NLayerApp.Caching
{
	public class ProductServiceWithCaching : IProductService
	{
		private const string CacheProductKey = "productsCache";
		private readonly IMapper _mapper;
		private readonly IMemoryCache _memoryCache;
		private readonly IProductRepository _repository;
		private readonly IUnitOfWork _unitOfWork;

		public ProductServiceWithCaching(IMapper mapper, IMemoryCache memoryCache, IProductRepository repository, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_memoryCache = memoryCache;
			_repository = repository;
			_unitOfWork = unitOfWork;

			if(!memoryCache.TryGetValue(CacheProductKey, out _))
			{
				memoryCache.Set(CacheProductKey, _repository.GetAll().ToList());
			}
		}

		public async Task<Product> AddAsync(Product entity)
		{
			await _repository.AddAsync(entity);
			await _unitOfWork.CommitAsync();
			CacheAll();
			return entity;
		}

		public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entites)
		{
			await _repository.AddRangeAsync(entites);
			await _unitOfWork.CommitAsync();
			CacheAll();
			return entites;
		}

		public Task<bool> AnyAsync(Expression<Func<Product, bool>> expression)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Product>> GetAllAsync()
		{
			return Task.FromResult(_memoryCache.Get<IEnumerable<Product>>(CacheProductKey));
		}

		public Task<Product> GetByIdAsync(int id)
		{
			var product = _memoryCache.Get<List<Product>>(CacheProductKey).FirstOrDefault(x => x.Id == id);

			if(product == null)
			{
				throw new NotFoundException($"{typeof(Product).Name} {id} not found");
			}

			return Task.FromResult(product);
		}

		public async Task<CustomResponseDto<List<ProductWithCategoryDto>>> GetProductWithCategory()
		{
			var products = await _repository.GetProductWithCategory();

			var productsWithCategory = _mapper.Map<List<ProductWithCategoryDto>>(products);

			return CustomResponseDto<List<ProductWithCategoryDto>>.Success(200,productsWithCategory);
		}

		public async Task RemoveAsync(Product entity)
		{
			_repository.Remove(entity);
			await _unitOfWork.CommitAsync();
			CacheAll();
		}

		public async Task RemoveRangeAsync(IEnumerable<Product> entites)
		{
			_repository.RemoveRange(entites);
			await _unitOfWork.CommitAsync();
			CacheAll();
		}

		public async Task UpdateAsync(Product entity)
		{
			_repository.Update(entity);
			await _unitOfWork.CommitAsync();
			CacheAll();
		}

		public IQueryable<Product> Where(Expression<Func<Product, bool>> expression)
		{
			return _memoryCache.Get<List<Product>>(CacheProductKey).Where(expression.Compile()).AsQueryable();
		}

		public void CacheAll()
		{
			_memoryCache.Set(CacheProductKey, _repository.GetAll().ToList());
		}
	}
}
