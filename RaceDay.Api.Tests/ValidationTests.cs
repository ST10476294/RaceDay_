using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Controllers;
using Xunit;
namespace RaceDay.Api.Tests;
public class ValidationTests
{
    private static bool Valid(object value) => Validator.TryValidateObject(value,new ValidationContext(value),new List<ValidationResult>(),true);
    [Theory]
    [InlineData(0,100,0,false)]
    [InlineData(10,0,0,false)]
    [InlineData(10,100,-1,false)]
    [InlineData(10,100,50,true)]
    public void Category_Values_EnforceDistanceCapacityAndFee(int distance,int capacity,int fee,bool expected)
        => Assert.Equal(expected,Valid(new CategoryRequest { Name="10km",DistanceKm=distance,MaxParticipants=capacity,EntryFee=fee }));
    [Fact]
    public void Event_PastDate_IsInvalid() => Assert.False(Valid(new EventRequest { Name="Race",Discipline="Running",Location="JHB",Description="Race",EventDate=DateTime.Now.AddDays(-1) }));
    [Theory]
    [InlineData("Finished",null,null,false)]
    [InlineData("Finished","00:10:00",1,true)]
    [InlineData("Finished","1.01:00:00",1,false)]
    [InlineData("DNF",null,null,true)]
    [InlineData("Unknown",null,null,false)]
    public void Result_StatusTimeAndPosition_AreValidated(string status,string? time,int? position,bool expected)
        => Assert.Equal(expected,Valid(new ResultRequest { Status=status,FinishTime=time==null?null:TimeSpan.Parse(time),OverallPosition=position,CategoryPosition=position }));
}
