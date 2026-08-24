using System.ComponentModel.DataAnnotations.Schema;
namespace POS.Shared.Entities;
public class DispatchDetail
{
 public int Id{get;set;} public int DispatchId{get;set;} public Dispatch? Dispatch{get;set;}
 public int ProductId{get;set;} public InventoryProduct? Product{get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal Quantity{get;set;}
}
