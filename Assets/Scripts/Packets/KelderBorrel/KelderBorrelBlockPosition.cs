using System;

public struct KelderBorrelBlockPosition : IEquatable<KelderBorrelBlockPosition> {
    private readonly int lineNumber;
    private readonly int blockX;

    public KelderBorrelBlockPosition(int lineNumber, int blockX) {
        this.lineNumber = lineNumber;
        this.blockX = blockX;
    }

    public int GetLineNumber() {
        return lineNumber;
    }

    public int GetBlockX() {
        return blockX;
    }

    public override bool Equals(object obj) {
        return obj is KelderBorrelBlockPosition position && Equals(position);
    }

    public bool Equals(KelderBorrelBlockPosition other) {
        return lineNumber == other.lineNumber &&
               blockX == other.blockX;
    }

    public override int GetHashCode() {
        int hashCode = 2033774249;
        hashCode = hashCode * -1521134295 + lineNumber.GetHashCode();
        hashCode = hashCode * -1521134295 + blockX.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(KelderBorrelBlockPosition left, KelderBorrelBlockPosition right) {
        return left.Equals(right);
    }

    public static bool operator !=(KelderBorrelBlockPosition left, KelderBorrelBlockPosition right) {
        return !(left == right);
    }
}
