using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace POS.Shared.Entities;
public static class InventoryMovementTypes { public const string ProductionEntry="EntradaProduccion"; public const string DispatchExit="SalidaDespacho"; }
public class InventoryMovement
{
 public int Id{get;set;} public int ProductId{get;set;} public InventoryProduct? Product{get;set;}
 public int UserId{get;set;} public User? User{get;set;} [Required,MaxLength(30)] public string MovementType{get;set;}="";
 [Column(TypeName="decimal(18,2)")] public decimal Quantity{get;set;} public DateTime Date{get;set;}
 [MaxLength(100)] public string Reference{get;set;}="";
}
