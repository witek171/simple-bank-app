﻿using AutoMapper;
using BankApp.DTOs;
using BankApp.Models;
using BankApp.Repositories;
using BankApp.Security;

namespace BankApp.Services;

public class ClientService
{
    private readonly ClientRepository _clientRepository;
    private readonly IMapper _mapper;

    public ClientService(
        ClientRepository clientRepository,
        IMapper mapper
    )
    {
        _clientRepository = clientRepository;
        _mapper = mapper;
    }

    public async Task<ClientResponse> FindById(long clientId)
    {
        var client = await _clientRepository.FindByIdAsync(clientId);
        ValidateClient(client);

        return _mapper.Map<ClientResponse>(client);
    }

    public async Task<Client?> Authenticate(
        string login,
        string password
    )
    {
        var client = await _clientRepository.FindByLoginAsync(login);
        ValidateClient(client);

        if (!PasswordHasher.Verify(password, client.Password))
            throw new ArgumentException("Invalid password.");

        return client;
    }

    public async Task Save(RegisterRequest registerRequest)
    {
        var existing = await _clientRepository
            .FindByLoginAsync(registerRequest.Login);

        if (existing != null)
            throw new InvalidOperationException("Login already taken.");

        var client = _mapper.Map<Client>(registerRequest);

        await _clientRepository.SaveAsync(client);
    }

    public async Task RemoveByLogin(string login)
    {
        var client = await _clientRepository.FindByLoginAsync(login);
        ValidateClient(client);

        if (client.GetBalance() > 0)
            throw new InvalidOperationException("Client balance must be zero before deletion.");

        await _clientRepository.DeleteAsync(client);
    }

    private static void ValidateClient(Client client)
    {
        if (client == null)
            throw new ArgumentException("Client not found.");
    }
}