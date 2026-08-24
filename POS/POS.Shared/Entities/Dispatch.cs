using System.ComponentModel.DataAnnotations;

namespace POS.Shared.Entities;
public static class DispatchStatuses { public const string Pending="Pendiente"; public const string Confirmed="Confirmado"; public const string Cancelled="Anulado"; }
public class Dispatch
{
 public int Id{get;set;} public int UserId{get;set;} public User? User{get;set;} public DateTime Date{get;set;}
 [Required,MaxLength(150)] public string Destination{get;set;}=""; [MaxLength(300)] public string Observation{get;set;}="";
 [Required,MaxLength(30)] public string Status{get;set;}=DispatchStatuses.Pending;
 public ICollection<DispatchDetail> Details{get;set;}=new List<DispatchDetail>();
}
