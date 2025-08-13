global using API.Controllers.Authentication.Models;

global using Application.Authentication.Services;
global using Application.Common.Interfaces;
global using Application.Options;
global using Application.Users.Services;

global using Domain.Common;
global using Domain.Users;

global using Infrastructure.ExternalApis.Espn;
global using Infrastructure.Persistence;
global using Infrastructure.Persistence.Repositories;
global using Infrastructure.Security;
global using Infrastructure.Services;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.OpenApi.Models;

global using Scalar.AspNetCore;
global using SportPicks.API.Authorization;
global using SportPicks.API.Configuration;
global using SportPicks.API.Middleware;
global using SportPicks.API.Models;

global using System.Security.Claims;
global using System.Text;
global using System.ComponentModel.DataAnnotations;global using System.ComponentModel.DataAnnotations;