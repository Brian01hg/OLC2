AS = aarch64-linux-gnu-as
LD = aarch64-linux-gnu-ld
CPU = cortex-a57

TARGET = program

# Ensamblador y enlazar
$(TARGET): $(TARGET).o
	$(LD) $< -o $@
	rm -f $<

%.o: %.s
	$(AS) -mcpu=$(CPU) $< -o $@

clean:
	rm -rf *.o $(TARGET)