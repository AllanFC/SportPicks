global using API.Controllers.Authentication.Models;
global using API.Controllers.Users.Models;

global using Application.Authentication.Services;
global using Application.Common.Interfaces;
global using Application.Options;
global using Application.Users.Services;

global using Infrastructure.Persistence;
global using Infrastructure.Persistence.Repositories;
global using Infrastructure.Security;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;

global using Scalar.AspNetCore;

global using System.Text;