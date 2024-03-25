
public class ConcilliationOutputDTO
{
  public ConcilliationFileContent[] DatabaseToFile { get; set; } = null!;
  public ConcilliationFileContent[] FileToDatabase { get; set; } = null!;
  public ConcilliationPaymentId[] DifferentStatus { get; set; } = null!;
}

public class ConcilliationFileContent
{
  public long Id { get; set; }
  public string Status { get; set; } = null!;
}

public class ConcilliationPaymentId
{
  public long Id { get; set; }
}