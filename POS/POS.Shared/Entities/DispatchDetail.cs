using System.ComponentModel.DataAnnotations.Schema;
namespace POS.Shared.Entities;
public class DispatchDetail
{
 public int Id{get;set;} public int DispatchId{get;set;} public Dispatch? Dispatch{get;set;}
 public int ProductId{get;set;} public InventoryProduct? Product{get;set;}
 public int? ProductionLotId{get;set;} public ProductionLot? ProductionLot{get;set;}
 public int Quantity{get;set;}
}
