using CrudApi.Models;
using CrudApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrudApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductsController : ControllerBase
    {
        private readonly Services.IProductStore _store;
        public ProductsController(IProductStore store) => _store = store;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            return  Ok(await _store.GetAllAsync());
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var p = await _store.GetByIdAsync(id);
            return p is null ? NotFound(new {message = "khong tim thay san pham co id nay "}) : Ok(p);           
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product p)
        {
            if(string.IsNullOrWhiteSpace(p.Name))
            {
                return BadRequest(new {message = "ten san pham khong duoc de trong"});
            }
            if (p.Price <= 0 || p.Stock <= 0)
            {
                return BadRequest(new { message = "so luong duoc nhap khong hop le " });
            }
            var created = await _store.CreateAsync(new Product
            {
                Name = p.Name.Trim(),
                Price = p.Price,
                Stock = p.Stock
            });
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product p)
        {
            if (string.IsNullOrWhiteSpace(p.Name))
            {
                return BadRequest(new { message = "ten san pham khong duoc de trong" });
            }
            if (p.Price <= 0 || p.Stock <= 0)
            {
                return BadRequest(new { message = "so luong duoc nhap khong hop le " });
            }
            var ok = await _store.UpdateAsync(p);
            return ok ? NoContent() : NotFound(new { message = "khong tim thay san pham co id nay " });      
        }


        [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _store.DeleteAsync(id);
            return ok ? NoContent() : NotFound(new { message = "khong tim thay san pham co id nay " });
        }
    }
}
