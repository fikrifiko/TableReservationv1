<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;

namespace Table_Reservation.Controllers
{
    public class ClientController : Controller
    {
        // Action pour afficher la page de réservation client
        public IActionResult PageReservationClient()
        {
            return View("PageReservationClient"); // Retourne la vue Page_Reservation_Client.cshtml
=======
﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Table_Reservation.Models;
using Table_Reservation.Services.Interfaces;

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IValidator<ClientModel> _validator;

        public ClientController(IClientService clientService, IValidator<ClientModel> validator)
        {
            _clientService = clientService;
            _validator = validator;
        }

        // Action pour afficher la page de réservation client
        [HttpGet("{id}")]
        public IActionResult GetClientById(int id)
        {
            return Ok(_clientService.GetClientById(id));
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            return Ok(_clientService.GetClients());
        }

        [HttpPost]
        public IActionResult AddClient([FromBody] ClientModel model)
        {
            var validationResult = _validator.ValidateAsync(model);

            if (model != null && !(validationResult.Result.IsValid))
            {
                return BadRequest(new
                {
                    Message = "Validation Failed",
                    Errors = validationResult.Result.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage
                    })
                });
            }

            return Ok(_clientService.AddClient(model));
        }

        [HttpPut]
        public IActionResult UpdateClient([FromBody] ClientModel model) 
        {
            var validationResult = _validator.ValidateAsync(model);

            if ((model != null && !(validationResult.Result.IsValid) )|| model.Id == 0)
            {
                return BadRequest(new
                {
                    Message = "Validation Failed",
                    Errors = validationResult.Result.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage
                    })
                });
            }

            return Ok(_clientService.UpdateClient(model));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            if(id == 0)
            {
                return BadRequest("Id cannot be 0");
            }

            _clientService.DeleteClient(id);

            return Ok();
>>>>>>> old-origin/master
        }
    }
}
