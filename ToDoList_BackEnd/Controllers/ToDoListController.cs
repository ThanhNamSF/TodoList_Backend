using Cassandra;
using Cassandra.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToDoList_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoListController : ControllerBase
    {
        private Cluster _cluster;
        private ISession _session;
        private IMapper _mapper;
        private readonly CassandraInformations _cassandraInformations;
        public ToDoListController(IOptions<CassandraInformations> options)
        {
            _cassandraInformations = options.Value;

            _cluster = ConnectCassandra();
            _session = _cluster.Connect("cycling");
            _mapper = new Mapper(_session);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _mapper.FetchAsync<ToDoList>("SELECT * FROM ToDoList");
            _session.Dispose();
            _cluster.Dispose();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ToDoList req)
        {
            var newItem = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = req.Name
            };

            await _mapper.InsertAsync(newItem);
            _session.Dispose();
            _cluster.Dispose();

            return Ok(newItem);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mapper.DeleteAsync<ToDoList>("WHERE id = ?", id);

            return Ok();
        }

        private Cluster ConnectCassandra()
        {
            var host = _cassandraInformations.Host;
            var cluster = Cluster.Builder()
                     .AddContactPoints(host)
                     .Build();

            return cluster;
        }
    }
}
