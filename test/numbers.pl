use strict;
use warnings;

use Fcntl 'SEEK_SET';

open my $fh, '<', '/tmp/anfs/numeric/data' or die "Cannot open test file: $!\n";
seek $fh, 5, SEEK_SET;
my $text = <$fh>;
chomp $text;
print "$text\n";
