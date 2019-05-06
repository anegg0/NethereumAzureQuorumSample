using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Quorum;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;
using Nethereum.StandardTokenEIP20;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;

namespace NethereumAzureQuorumSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            new QuorumPrivateTest().Run().Wait();
            new QuorumNormalTest().Run().Wait();
        }
    }

    public class QuorumPrivateTest
    {
        public async Task Run()
        {
            //The quorum account
            var coinbaseNode1 = "0x83e0ebe69d8758f9450425fa39ef08692e55340d";
            var uriWithAccessTokenNode1 = "https://juanquorum1.blockchain.azure.com:3200/nNkcS8DyDSCLIC9oAoCw1orS";

            //Initialising Web3Quorum with a custom QuorumAccount
            var web3Private = new Web3Quorum(new QuorumAccount(coinbaseNode1), uriWithAccessTokenNode1);

            //Set the nodes to work in private mode for this web3 instance
            web3Private.SetPrivateRequestParameters(new[] { "LHTjKEqQPy6gbo4r9ouj8ztfbB+F7kWd9vosSmeQcEw=", "sXVr5ENaJeqAA8eTKm74f6epYTMcbsl8Ovp+Y8Q3dzA=" });

            //Unlock account to enable access
            var unlocked = await web3Private.Personal.UnlockAccount.SendRequestAsync(coinbaseNode1, "P455word1?1234", 30);

            //Deploying new ERC20 smart contract using the Standard token library service
            var erc20service = await StandardTokenService.DeployContractAndGetServiceAsync(web3Private, new EIP20Deployment()
            {
                InitialAmount = BigInteger.Parse("1000000000000000000000000"),
                DecimalUnits = 18,
                TokenName = "TEST",
                TokenSymbol = "TST",
            });

            //After deploying the smart contract the owner "coinbaseNode1" will have a balance of 1000000000000000000000000
            var balanceOwnerAccount = await erc20service.BalanceOfQueryAsync(coinbaseNode1);

            //Transfering 10000
            var transferReceipt = await erc20service.TransferRequestAndWaitForReceiptAsync("0xc45ed03295fdb5667206c4c18f88b41b4f035358", 10000);

            //Validate that we get the new balance
            var balanceOwnerAfterTransfer = await erc20service.BalanceOfQueryAsync(coinbaseNode1);
            var balanceReceiverAccount = await erc20service.BalanceOfQueryAsync("0xc45ed03295fdb5667206c4c18f88b41b4f035358");

            //Create a web3 instance in a different node not included in the Private list
            var web3NoAccess = new Web3("https://juanquorum3-juanquorum1.blockchain.azure.com:3200/ekgKWCEhxcq6d_HH3N10g9W0");

            //initialising a new StardandTokenService with the same contract address
            var erc20noAccess = new StandardTokenService(web3NoAccess, erc20service.ContractHandler.ContractAddress);
            //validate we don't receive any amount
            var balanceOwnerAccountNoAccess = await erc20noAccess.BalanceOfQueryAsync(coinbaseNode1);

        }
    }

    public class QuorumNormalTest
    {
        public async Task Run()
        {
            //Basic Authentication
            var web3 = new Web3("https://juanmemberq1:p455word@juanmemberq1.blockchain.azure.com:3200");
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();


            //access keys
            var web32 = new Nethereum.Web3.Web3("https://juanmemberq1.blockchain.azure.com:3200/QNSQSAAE_WoMyS06TPH8KVa2");
            blockNumber = await web32.Eth.Blocks.GetBlockNumber.SendRequestAsync();


            //access keys websocket
            var websocketClient =
                new WebSocketClient(("wss://juanmemberq1.blockchain.azure.com:3300/QNSQSAAE_WoMyS06TPH8KVa2"));

            var web3WebSocket = new Web3(websocketClient);
            blockNumber = await web3WebSocket.Eth.Blocks.GetBlockNumber.SendRequestAsync();


            //member account
            var accounts = await web3.Eth.Accounts.SendRequestAsync();

            //0x411d5607e9bad791efbb4f50669a70879faf5656

            var managedAccount = new ManagedAccount("0xca1e76c9876e5ba1e7c307696a7ea48eb25eec8c", "p455word");
            var web3Managed = new Web3(managedAccount, "https://juanmemberq1.blockchain.azure.com:3200/QNSQSAAE_WoMyS06TPH8KVa2");
            var balance = await web3Managed.Eth.GetBalance.SendRequestAsync("0xca1e76c9876e5ba1e7c307696a7ea48eb25eec8c");
            var rootAddressBalance = await web3Managed.Eth.GetBalance.SendRequestAsync("0xfb091e4feceec1869daba758de9e9ef00c2e4d7a");

            var service = await StandardTokenService.DeployContractAndGetServiceAsync(web3Managed, new EIP20Deployment()
            {
                InitialAmount = BigInteger.Parse("1000000000000000000000000"),
                DecimalUnits = 18,
                TokenName = "TEST",
                TokenSymbol = "TST"
            });

            var receipt = await service.TransferRequestAndWaitForReceiptAsync("0x12890d2cce102216644c59daE5baed380d84830c", 10000000);


            var web3MyAccount = new Web3(new Account("0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7"), "https://juanmemberq1.blockchain.azure.com:3200/QNSQSAAE_WoMyS06TPH8KVa2");
            var service2 = new StandardTokenService(web3MyAccount, service.ContractHandler.ContractAddress);
            var receipt2 = await service2.TransferRequestAndWaitForReceiptAsync("0x12890d2cce102216644c59daE5baed380d84830d", 10000);
            var balanceMyAccount = await service2.BalanceOfQueryAsync("0x12890d2cce102216644c59daE5baed380d84830c");


            var websocketClient2 =
               new WebSocketClient(("wss://juanmemberq1.blockchain.azure.com:3300/QNSQSAAE_WoMyS06TPH8KVa2"));

            var web3WebSocket2 = new Web3(new Account("0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7"), websocketClient);

            var service3 = await StandardTokenService.DeployContractAndGetServiceAsync(web3WebSocket2, new EIP20Deployment()
            {
                InitialAmount = BigInteger.Parse("1000000000000000000000000"),
                DecimalUnits = 18,
                TokenName = "TEST",
                TokenSymbol = "TST"
            });
            var receipt3 = await service3.TransferRequestAndWaitForReceiptAsync("0x12890d2cce102216644c59daE5baed380d84830d", 10000);
            var balanceMyAccount2 = await service2.BalanceOfQueryAsync("0x12890d2cce102216644c59daE5baed380d84830c");

        }
    }

}
